# 仓库总览 · Zheng Shang You / 争上游

一句话：这是一个**能直接玩的三人扑克牌「争上游」小游戏**——5 局定胜负、真实牌面、8 种语言，同时提供 Windows 单文件版、便携版、安装版和安卓 APK。源码、文档、截图和四个成品都在这个仓库里，克隆或解压即可运行，也可用一条命令从源码重新构建。

## 1. 这是个什么游戏

三个人（你 + 两个电脑）分一副 54 张牌，每人 18 张，先把牌出完的是「上游」，第二个是「中游」，最后剩牌的是「下游」，共打 5 局。

| 规则 | 说明 |
| --- | --- |
| 牌力（从大到小） | `5 > 8 > 大王 > 小王 > 3 > 2 > A > K > Q > J > 10 > 9 > 7 > 6 > 4`（15 档） |
| 允许的牌型 | 单张、对子、三带一（4 张）；仅当手里恰好剩三张同点时，可裸打「三张」 |
| 不存在的牌型 | 炸弹、飞机、连对、顺子，均不可出 |
| 首局定先手 | 持红桃 4 的人先出，且第一手必须包含红桃 4（三带一或对子皆可） |
| 出牌位置 | 出过的牌一律摆到牌桌中央，并显示「谁出了什么牌型」；三家都不要时提示由谁重新领出 |
| 换牌（第 2 局起） | 第一步「上贡」：下游把最大牌给中游、中游把最大牌给上游（按上贡前手牌计算）；第二步「要牌」：可向对方索要一张凑对子/三带一，对方有且愿意才给，最多拒绝 3 次 |
| 要牌限制 | 不得索要对方当前手里最大的牌；不得索回刚上贡出去的那张牌 |
| 思考时间 | 每手 30 秒，超时自动 pass；轮到你领出而超时时，自动打出最小合法牌 |
| 名次 | 每局记录上游/中游/下游，5 局全部打完出总成绩单 |

## 2. 四个可运行的东西

| 版本 | 文件 | 适合谁 | 怎么用 |
| --- | --- | --- | --- |
| 桌面版 | `ZhengShangYou.exe` | 想立刻双击开玩 | 双击即运行，不写系统目录；语言设置存 `%APPDATA%\ZhengShangYou\settings.ini` |
| 便携版 | `ZhengShangYou-Portable.zip` | 放 U 盘、换电脑 | 解压后双击 `ZhengShangYou.exe`；因带 `portable.txt`，设置改存同目录 `settings.ini` |
| 安装版 | `ZhengShangYou-Setup.exe` | 想有桌面/开始菜单图标、能像普通软件一样卸载 | 运行安装器（自带 8 语言界面），装到 `%LOCALAPPDATA%\Programs\ZhengShangYou`，写 HKCU 卸载项 |
| 安卓版 | `ZhengShangYou.apk` | 手机上玩 | 传到手机安装即可（minSdk 21 / Android 5.0+），横屏触屏牌桌 |

桌面三件套共用一份规则引擎和一份语言表；安卓版把规则引擎重写成纯 Java（与桌面同规则、同文案），界面换成触屏自绘 View。

## 3. 八种语言，切了就全切

支持：简体中文、繁體中文、English、Français、Русский、日本語、Español、العربية。

切换不是只换标题，而是**界面文案、规则正文、对局日志、弹窗提示、名次与总成绩单一起换**；长翻译自动缩字号而不是被裁掉；阿拉伯语整屏切到 RTL（桌面端文字右对齐、按钮与信息面板左右互换，安卓端面板与弹窗整体镜像）。

入口分布：

* 桌面版：开始页的「Language / 语言」按钮、规则页按钮、对局中的菜单按钮，命令行也可 `-lang=xx`。
* 安装器：右上角语言下拉框，8 种语言即时刷新，阿拉伯语窗口整体右起；装完按所选语言启动游戏。
* 安卓版：牌桌顶栏语言按钮（阿拉伯语时自动移到左上）、开始弹窗的语言按钮、菜单里的「语言」；桌面图标名也做了本地化（争上游 / 爭上游 / Zheng Shang You）。

语言只有一份原始数据：`Lang.cs`（108 个 key × 8 种语言）。桌面直接编译它；安卓由 `generate-lang.ps1` 逐字符解析后生成 `Lang.java`，所以两端文案不会各自漂移。

## 4. 技术实现（为什么这么小、这么独立）

* **桌面**：C# + WinForms 全自绘，用 Windows 自带的 `csc.exe` 把 5 个 `.cs` 编译成一个约 130 KB 的 winexe。没有 SDK、没有 NuGet、没有第三方 DLL；扑克牌面用 GDI+ 现画（牌点、花色、大小王、牌背「Z」），所以任意 DPI 都清晰，也不含任何受版权保护的牌图。
* **安卓**：不引入 Gradle / AGP / Kotlin。规则引擎是纯 Java（不含 `android.*` 导入），因此能直接在桌面 JVM 上跑 300 局无人值守自测；界面是一个自绘 `View`，构建走 `javac → d8 → aapt2 → zipalign → apksigner` 的手工链路。
* **安装器**：另一个 WinForms 小程序，用 `csc /resource:` 把游戏 exe 内嵌为 `PAYLOAD`，自复制成 `Uninstall.exe`，卸载时用隐藏 cmd 延时删目录（运行中的 exe 删不掉自己）。
* **CI**：`ci-build.yml` 两个 job——Windows 上重建桌面三件套并跑 `-selftest`，以及安装 JDK 17 + Android SDK 后重建 APK，产物作为 artifact 上传。

## 5. 目录地图

```
README.md / README.zh-CN.md   仓库门面：功能、下载、截图、构建、语言说明
LICENSE                       MIT
CHANGELOG.md                  版本变更记录
src/                          桌面端 5 个 C# 源文件（规则 / 语言表 / 牌面 / 界面 / 主循环）
android/                      安卓工程：Java 引擎 + 自绘界面 + 资源 + 手工构建脚本
build/                        桌面编译、打包、8 语言安装器源码
docs/                         RULES / BUILD / ANDROID / OVERVIEW（本文）/ FILES
assets/                       图标与 16 张截图（8 语言牌桌、规则页、安装器）
dist/                         四个可直接运行的成品
.github/workflows/            持续集成
```

每个文件的用途、大小、什么时候会改动：见 [FILES.md](FILES.md)。

## 6. 构建与自检

```powershell
powershell -ExecutionPolicy Bypass -File build\package.ps1            # exe + 便携 zip + setup
powershell -ExecutionPolicy Bypass -File android\build\build-apk.ps1  # APK
```

自检命令（桌面版内置）：

| 命令 | 作用 |
| --- | --- |
| `ZhengShangYou.exe -selftest` | 无人值守打 5 局，输出 `selftest.txt`：首局首手含红桃 4、每次发牌/换牌后合计 54 张、5 局名次 |
| `ZhengShangYou.exe -snapshot` | 渲染开始页/牌桌/弹窗/名次/规则/语言六张 png，用于目视检查排版 |
| `ZhengShangYou.exe -preview` | 渲染牌面样张 |
| `ZhengShangYou.exe -lang=xx` | 强制语言（zh-CN zh-TW en fr ru ja es ar） |

安卓引擎的等价自测（不需要设备）：

```powershell
& build\tools\jdk\bin\java.exe -cp build\tools\jtest com.zhengshengyou.engine.SelfTest
# SELFTEST OK: 300 matches, rules and languages verified
```

## 7. 验证到什么程度，以及哪里没验证

已经做过：

* 桌面版 `-selftest` 通过（5 局名次、发牌与换牌后各 54 张、首局红桃 4 规则）。
* 桌面版 8 种语言 `-snapshot` 逐张目视检查，含阿拉伯语 RTL 规则页与开始页的修正。
* 安装器：静默安装 → 运行游戏 → 卸载全链路实跑，目录、两个快捷方式、注册表项均正确创建与清除；8 种语言窗口逐张截图目视（英文 / 日文 / 俄文 / 阿拉伯语）。
* APK：`apksigner verify` 通过、`aapt2 dump badging` 包名/SDK 级别/5 档图标/启动 Activity 正确、dex 含全部类、引擎 300 局自测通过。

尚未验证：

* 安卓的触摸界面没有在真机或模拟器上实际走过一遍（构建环境无设备）。规则与状态机与桌面同源，差异只在排版，而排版参数集中在 `TableView.java` 的一处 `dp` 计算里，便于微调。
* APK 使用调试密钥签名，若要上架应用商店需换自己的 release keystore。

## 8. 版本、许可与反馈

* 当前版本：`1.0.0`（见 [CHANGELOG.md](CHANGELOG.md)）。
* 许可：[MIT](LICENSE)，可自由使用、修改、分发；牌面为程序绘制，不含第三方素材。
* 发现排版或规则问题：桌面端先跑 `-snapshot` 定位是文案还是布局，安卓端改 `TableView.java` 后重跑 `android\build\build-apk.ps1`。
