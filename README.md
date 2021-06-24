### Ⅰ. 进击的坦克（The Fight of Tanks）

- 游戏的unity源码主要来自于 [超镦逸](https://space.bilibili.com/8355981) ，我对游戏做了二次开发，升级为了网络游戏
- [tank-game-server](https://github.com/zfoo-project/tank-game-server) 的unity客户端演示项目

### Ⅱ. 基础框架
- 游戏的基础框架使用了 [EllanJiang GameFramework](https://github.com/EllanJiang/GameFramework) ，并将GameFramework和UnityGameFramework合二为一，并减少了60%的的代码，并且加了GF的依赖注入IOC实现

### Ⅲ. 网络
- 通信协议的序列化框架使用 [protocol](https://github.com/zfoo-project/zfoo/tree/main/protocol) 目前性能最好的C#序列化和反序列化库
- 通信的网络层框架参考了 [Mirror Tcp](https://github.com/vis2k/Mirror) ，并将TcpClient移植到了本项目中
- 在浏览器中的通信使用浏览器原生的websocket，在本项目中封装了WebsocketClient