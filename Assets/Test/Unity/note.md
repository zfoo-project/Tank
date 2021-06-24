# 一、unity学习文档
- 顶点吸附，选择物体后按住vwe键，定位定点

- Project -> Scene -> GameObject -> Component

# 二、组件
## 1.材质Rendering Mode
- Opaque，不透明，默认选线
- Cutout，镂空，用于完全透明或完全不透明的物体，如栅栏
- Transparent，透明，用于半透明或全透明物体，如玻璃
- Fade，渐变，用于需要淡入淡出的物体

```
Material是表现Shading的数据集，在unity上看就是一个材质Inspector面板。

Shader：着色器，专门用来渲染3D图形的技术，可以使纹理以某种方式展现。实际就是一段嵌入到渲染管线中的程序，可以控制GPU运算图像效果的算法。

Texture：纹理，附加到物体表面的贴图。
```

```
Albedo，基础贴图，决定物体表面纹理与颜色
Metallic，金属，是用金属特性模拟外观
Specular，镜面反射，使用镜面特性模拟外观
Smoothness，光滑度，设置物体表面光滑程度
Normal Map，法线贴图，描述物体表面凹凸程度
Emission，自发光，控制物体表面自发光颜色和贴图
Tiling，平铺
Offset，偏移
```

## 2.摄像机
- 快捷键，将当前Object定位到Scene的当前视野，ctrl + shift + f

## 3.碰撞器
- 可以使物体有物理边界，刚体通过这个边界检测碰撞。
- 碰撞器勾选了isTrigger后叫触发器
```
触发条件：
两者具有碰撞组件
其中之一带有刚体组件
其中之一勾选isTrigger
```

## 4.刚体
- 引擎会检测刚体的碰撞，使物体产生碰撞效果。可以使物体有重量，受重力影响，物理效果，物理特性。
```
碰撞产生的两个条件：
两者具有碰撞组件
运动的物体具有刚体组件
```

# 3、Unity脚本
```
// 序列化字段，在把编辑器中显示私有变量
[SerializeField]
private int a;

// 在编辑器中隐藏字段
[HideInspector]
private int a;

// 在把编辑器中一个范围调整变量
[Range(0, 100)]
private int a;
```
## 生命周期
```
Awake -> 创建游戏对象，立即执行；常用于游戏开始前的初始化，可以判断当满足某种条件执行此脚本（this.enable = true）。
OnEnable -> 脚本启用，立即执行，在Awake后立即执行，紧跟着Awake
Start -> 创建游戏对象，脚本启用，立即执行
OnBecomeVisible -> 当Mesh Renderer在任何相机上可见时调用
OnBecomeInvisible -> 当Mesh Renderer在任何相机上都不可见时调用
OnDisable -> 当游戏对象变为非激活状态（this.enable = false）被调用
OnDestory -> 当游戏对象被销毁时被调用
OnApplicationQuit -> 应用程序退出时被调用

FixedUpdate -> 执行时机：每隔固定时间执行一次约0.02秒，约每秒执行30次。适用性：适合对物体做物理操作（移动，旋转），不会受到渲染的影响
Update -> 执行时机：渲染帧执行，处理游戏逻辑
```

## 类class
- Time.deltaTime，每帧的时间

## 四元数
- Quaternion在3D数学中表示旋转，由一个三维向量xyz和一个标量w组成，四个的取值范围为[-1, 1]。
- 若旋转轴为V，旋转弧度为d，则：
```
x=sin(d/2)*V.x
y=sin(d/2)*V.y
z=sin(d/2)*V.z
w=cos(d/2)
```
- 四元数左乘向量，表示将该向量按照四元数表示的角度旋转。

# GF的一些问题
- 文件如果包含空格，无法打包