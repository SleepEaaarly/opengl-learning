#include <glad/glad.h>
#include <GLFW/glfw3.h>
#include <glm/glm.hpp>
#include <glm/gtc/matrix_transform.hpp>
#include <glm/gtc/quaternion.hpp>
#include <vector>
#include <string>
#include <iostream>
#include <fstream>
#include <sstream>

// 3D模型解析类，封装以解析的模型信息
class ImportedModel {
private:
    int _numVertices; // 顶点总数
    std::vector<glm::vec3> _vertices; // 顶点坐标，顶点包含(x, y, z)
    std::vector<glm::vec2> _texCoords; // 纹理坐标 (u, v)
    std::vector<glm::vec3> _normalVecs; // 顶点法线

public:
    ImportedModel() {}
    ImportedModel(const std::string& filePath, const char* format);

    // 返回模型数据
    int getNumVertices();
    std::vector<glm::vec3> getVertices();
    std::vector<glm::vec2> getTextureCoords();
    std::vector<glm::vec3> getNormals();
};

// 3D模型导入类，负责实际三维模型文件解析
class ModelImporter {
private:
    std::vector<float> _vertVals; // 原始顶点坐标
    std::vector<float> _stVals; // 原始纹理坐标
    std::vector<float> _normVals; // 原始法线向量

    std::vector<float> _triangleVerts; // 处理后顶点坐标
    std::vector<float> _textureCoords; // 处理后纹理坐标
    std::vector<float> _normals; // 处理后法线向量

public:
    ModelImporter() {}

    // 解析二进制STL文件
    void parseSTL_Binary(const std::string& filePath);
    // 解析ASCII STL文件
    void parseSTL_ASCII(const std::string& filePath);
    // 解析OBJ文件
    void parseOBJ(const std::string& filePath);
    // 解析3DS文件
    void parse3DS(const std::string& filePath);

    int getNumVertices();
    std::vector<float> getVertices();
    std::vector<float> getTextureCoordinates();
    std::vector<float> getNormals();
};

// 3D模型文件加载
std::vector<float> loadModel(ImportedModel& MyModel) {
    std::vector<float> vertexData;

    const auto& vertices = MyModel.getVertices();
    const auto& normals = MyModel.getNormals();

    for (size_t i = 0; i < vertices.size(); ++i) {
        // 顶点坐标
        vertexData.push_back(vertices[i].x);
        vertexData.push_back(vertices[i].y);
        vertexData.push_back(vertices[i].z);

        // 法线
        if (i < normals.size()) { // 确保法线索引不越界
            vertexData.push_back(normals[i].x);
            vertexData.push_back(normals[i].y);
            vertexData.push_back(normals[i].z);
        } else {
            // 若没有法线数据，填充默认值
            vertexData.push_back(0.0f);
            vertexData.push_back(0.0f);
            vertexData.push_back(0.0f);
        }
    }

    return vertexData;
}

// Arcball类
class Arcball {
private:
    float screenWidth, screenHeight;
    bool dragging;
    glm::vec3 startVector;
    glm::quat rotationQuat;

public:
    Arcball() {}
    Arcball(float width, float height);

    void setSize(float width, float height);
    float getWidth();
    float getHeight();
    void onMouseDown(float x, float y);
    void onMouseMove(float x, float y);
    void onMouseUp();
    glm::mat4 getRotationMatrix();
    glm::vec3 mapToSphere(float x, float y);
};

// 渲染循环
void renderLoop(Arcball& arcball, GLFWwindow* window, const std::vector<float>& vertices, GLuint VAO, GLuint shaderProgram) {
    float width = arcball.getWidth();
    float height = arcball.getHeight();

    while (!glfwWindowShouldClose(window)) {
        int w, h;
        glfwGetFramebufferSize(window, &w, &h);
        arcball.setSize(w, h);

        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
        glLoadIdentity();
        glUseProgram(shaderProgram);

        // 计算变换矩阵
        glm::mat4 model = glm::mat4(1.0f);
        model = glm::translate(model, glm::vec3(0.0f, 0.0f, -5.0f)); // 应用平移
        model *= arcball.getRotationMatrix(); // 应用Arcball旋转
        model = glm::scale(model, glm::vec3(0.5f)); // 应用缩放

        glm::mat4 view = glm::lookAt(glm::vec3(0.0f, 0.0f, 5.0f), glm::vec3(0.0f, 0.0f, 0.0f), glm::vec3(0.0f, 1.0f, 0.0f));
        glm::mat4 projection = glm::perspective(glm::radians(45.0f), static_cast<float>(w) / static_cast<float>(h), 0.1f, 100.0f);

        // 传递变换信息
        glUniformMatrix4fv(glGetUniformLocation(shaderProgram, "model"), 1, GL_FALSE, glm::value_ptr(model));
        glUniformMatrix4fv(glGetUniformLocation(shaderProgram, "view"), 1, GL_FALSE, glm::value_ptr(view));
        glUniformMatrix4fv(glGetUniformLocation(shaderProgram, "projection"), 1, GL_FALSE, glm::value_ptr(projection));

        glBindVertexArray(VAO);
        glDrawArrays(GL_TRIANGLES, 0, vertices.size() / 6);

        glfwSwapBuffers(window);
        glfwPollEvents();
    }
}

int main() {
    // 初始化GLFW和GLEW
    if (!glfwInit()) {
        std::cerr << "Failed to initialize GLFW" << std::endl;
        return -1;
    }

    GLFWwindow* window = glfwCreateWindow(800, 600, "OpenGL Window", nullptr, nullptr);
    if (!window) {
        std::cerr << "Failed to create GLFW window" << std::endl;
        glfwTerminate();
        return -1;
    }

    glfwMakeContextCurrent(window);
    glewExperimental = GL_TRUE;
    if (glewInit() != GLEW_OK) {
        std::cerr << "Failed to initialize GLEW" << std::endl;
        return -1;
    }

    std::cout << "OpenGL version: " << glGetString(GL_VERSION) << std::endl;

    // 创建Arcball对象
    Arcball arcball(800.0f, 600.0f);

    // 加载模型
    ImportedModel model("model.obj", "obj");
    std::vector<float> vertexData = loadModel(model);

    // 创建VAO和VBO
    GLuint VAO, VBO;
    glGenVertexArrays(1, &VAO);
    glGenBuffers(1, &VBO);
    glBindVertexArray(VAO);
    glBindBuffer(GL_ARRAY_BUFFER, VBO);
    glBufferData(GL_ARRAY_BUFFER, vertexData.size() * sizeof(float), vertexData.data(), GL_STATIC_DRAW);

    // 配置顶点属性
    glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 6 * sizeof(float), (void*)0);
    glEnableVertexAttribArray(0);
    glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, 6 * sizeof(float), (void*)(3 * sizeof(float)));
    glEnableVertexAttribArray(1);

    // 创建着色器程序
    GLuint vertexShader = glCreateShader(GL_VERTEX_SHADER);
    GLuint fragmentShader = glCreateShader(GL_FRAGMENT_SHADER);
    const char* vertexShaderSource = R"(
        #version 330 core
        layout (location = 0) in vec3 aPos;
        layout (location = 1) in vec3 aNormal;
        uniform mat4 model;
        uniform mat4 view;
        uniform mat4 projection;
        out vec3 FragPos;
        out vec3 Normal;
        void main()
        {
            FragPos = vec3(model * vec4(aPos, 1.0));
            Normal = mat3(transpose(inverse(model))) * aNormal;
            gl_Position = projection * view * vec4(FragPos, 1.0);
        }
    )";
    const char* fragmentShaderSource = R"(
        #version 330 core
        out vec4 FragColor;
        in vec3 FragPos;
        in vec3 Normal;
        uniform vec3 lightPos;
        void main()
        {
            vec3 norm = normalize(Normal);
            vec3 lightDir = normalize(lightPos - FragPos);
            float diff = max(dot(norm, lightDir), 0.0);
            vec3 result = diff * vec3(1.0, 1.0, 1.0);
            FragColor = vec4(result, 1.0);
        }
    )";
    glShaderSource(vertexShader, 1, &vertexShaderSource, NULL);
    glShaderSource(fragmentShader, 1, &fragmentShaderSource, NULL);
    glCompileShader(vertexShader);
    glCompileShader(fragmentShader);
    GLuint shaderProgram = glCreateProgram();
    glAttachShader(shaderProgram, vertexShader);
    glAttachShader(shaderProgram, fragmentShader);
    glLinkProgram(shaderProgram);
    glDeleteShader(vertexShader);
    glDeleteShader(fragmentShader);

    // 设置光源位置
    glUniform3fv(glGetUniformLocation(shaderProgram, "lightPos"), 1, glm::value_ptr(glm::vec3(0.0f, 0.0f, 2.0f)));

    // 渲染循环
    renderLoop(arcball, window, vertexData, VAO, shaderProgram);

    // 清理资源
    glDeleteVertexArrays(1, &VAO);
    glDeleteBuffers(1, &VBO);
    glfwDestroyWindow(window);
    glfwTerminate();
    return 0;
}