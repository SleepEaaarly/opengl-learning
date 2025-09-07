#include <iostream>
#include <fstream>
#include <string.h>
void rgbe2float32(const char *rgbe, float *out) noexcept;

bool isRadianceFile(std::ifstream& fs) {
    char tmp[12];

    fs.read(tmp, 11);
    tmp[11] = 0;
    
    if (strcmp("#?RADIANCE\n", tmp) != 0)
        return false;
    return true;
}

bool getToken(std::ifstream& fs, char* buff, size_t buff_size) {
    
    bool ret = true;
    char tmp, *pdat;
retry:
    pdat = buff;
    while (pdat < buff + buff_size - 1)
    {
        fs.read(&tmp, 1);
        if (fs.gcount() < 1)
        {
            ret = false;
            break;
        }
        if (tmp == '\n' || tmp == '\0')
        {
            *pdat = '\0';                                       // 封闭字符串
            break;
        }
        else {
            *pdat = tmp;
            pdat += 1;
        }
    }
    // assert(pdat < buff + buffsz);
    if (tmp != '\0' && (buff[0] == '#' || buff[0] == '\0'))     // 注释行或者空行, 重新再来一次
    {
        goto retry;
    }
    return ret;
}

bool readData(std::ifstream& fs, int imgw, int imgh, float* output)
{
    int i;
    bool ret = true;
    char temp[4], *scanline;
    scanline = new char[4 * imgw];
    if (imgw < 8 || imgw > 0x7fff)                              // 未编码数据直接读取
    {
        // ret = readRGBEPixel((size_t)(imgw * imgh));
        char tmp[4];
        for (size_t i = 0u; i < imgw*imgh; ++i) {
            fs.read(tmp, 4);
            if (fs.gcount() < 4) {
                ret = false;
                goto err;
            }
            rgbe2float32(tmp, output);
            output += 3;
        }
    }
    else {                                                      // 读取所有数据, 按照扫描线读取
        for (i = 0; i < imgh; i += 1)
        {
            fs.read(temp, 4);
            if (fs.gcount() < 4) {
                ret = false;
                goto err;
            }

            if (temp[0] != 2 || temp[1] != 2 || (temp[2] & 0x80))
            {                                                   // 该部分未使用rle编码
                rgbe2float32(temp, output);
                output += 3;
                // ret = readRGBEPixel((size_t)(imgw * imgh - 1)); // (已经读取过一个像素了, 所以 - 1)
                char tmp[4];
                for (size_t i = 0u; i < imgw*imgh-1; ++i) {
                    fs.read(tmp, 4);
                    if (fs.gcount() < 4) {
                        ret = false;
                        goto err;
                    }
                    rgbe2float32(tmp, output);
                    output += 3;
                }                                               // (已经读取过一个像素了, 所以 - 1)
                break;
            }
            if ((((int)temp[2]) << 8 | temp[3]) != imgw)        // 错误的编码 
            {
                ret = false;
                break;
            }
            char buff[2];
            for (int i = 0; i < 4; i += 1)                      // 读取四个通道的数据到scanline
            {
                char *pdat = scanline + (i + 0) * imgw;
                char *pend = scanline + (i + 1) * imgw;
                while (pdat < pend)
                {
                    fs.read(buff, 2);
                    if (fs.gcount() < 2)
                    {
                        ret = false;
                        goto err;
                    }
                    if (buff[0] > 128)                          // 一小块相同值的数据
                    {
                        int count = (int)buff[0] - 128;
                        if ((count == 0) || (count > pend - pdat))
                        {                                       // 不对劲的块
                            ret = false;
                            goto err;
                        }
                        while (count-- > 0) {
                            *pdat = buff[1];
                            pdat += 1;
                        }
                    }
                    else {                                      // (啥也不是)
                        int count = (int)buff[0];
                        if ((count == 0) || (count > pend - pdat))
                        {                                       // 不对劲的块
                            ret = false;
                            goto err;
                        }
                        *pdat = buff[1];
                        pdat += 1;
                        count -= 1;
                        if (count > 0)
                        {   
                            fs.read(pdat, (size_t)count);
                            if (fs.gcount() < count) {
                                ret = false;
                                goto err;
                            }
                            pdat += count;
                        }
                    }
                }
            }
            char tmp[4];
            for (size_t i = 0u; i < imgw; i += 1) {
                tmp[0] = scanline[i + imgw * 0];
                tmp[1] = scanline[i + imgw * 1];
                tmp[2] = scanline[i + imgw * 2];
                tmp[3] = scanline[i + imgw * 3];                           // 调整出rgbe顺序的数据
                rgbe2float32(tmp, output);
                output += 3;                                          // rgb三个通道, +=3
            }              
            // 把这行rgbe转换为float
        }
    }
err:
    delete[] scanline;
    return ret;
}

// rgbe -> rgb
// 输入格式
// pdat:
// [ 
//     R, R, ......(count个),
//     G, G, ......(count个),
//     B, B, ......(count个),
//     E, E, ......(count个),
// ]
// 将rgbe数据转换为rgb, 存放格式如下:
// float[0] : R
// float[1] : G
// float[2] : B
// 输入按照RGBE顺序存放
void rgbe2float32(const char *rgbe, float *out) noexcept
{
    if (rgbe[3] == 0)                                           // 指数位是0, rgb都是0
    {
        out[0] = 0.0f;
        out[1] = 0.0f;
        out[2] = 0.0f;
    }
    else {
        const int E = (int)rgbe[3] - 128 - 8;                   // 指数位的值
        const double P = ldexp(2.0, E);                         // 2的E次幂的结果
        out[0] = (float)((double)rgbe[0] * P);                  // 计算三个通道的值
        out[1] = (float)((double)rgbe[1] * P);
        out[2] = (float)((double)rgbe[2] * P);
    }
}

float* loadHDRFile(const char* path, int* w, int* h) {
    char buff[128];
    std::ifstream fs(path);
    int width, height;
    if (!fs) {
        std::cout << "!fs" << std::endl;
        return nullptr;
    }

    if (isRadianceFile(fs) == false) {
        std::cout << "not radiance file" << std::endl;
        return nullptr;
    }
    // 
    if (getToken(fs, buff, 128) == false)
    {
        std::cout << "token error" << std::endl;
        return nullptr;
    }
    if (strcmp("FORMAT=32-bit_rle_rgbe", buff) != 0)
    {
        std::cout << "format=32...error" << std::endl;
        return nullptr;                                           // 不支持的文件格式
    }
    //
    do                                                          // 连续读取, 直到找到-Y XXX这个尺寸信息
    {
        if (getToken(fs, buff, 128) == false)
        {
            return nullptr;
        }
    } while (buff[0] != '-' || buff[1] != 'Y');
    int sz = sscanf_s(buff, "-Y %d +X %d", &height, &width);
    *h = height;
    *w = width;
    if(sz != 2)                                                 // 不符合规范的尺寸数据
    {
        return nullptr;
    }
    float *data = new float((*w)*(*h)*3);
    auto ret = readData(fs, *w, *h, data);
    if (ret == false) {
        std::cout << "readdata error" << std::endl;
        return nullptr;
    }
    return data;
}

