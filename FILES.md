# 文件说明 · File reference

这个文档逐个说明**除源码之外**的文件：它是干什么的、大概多大、什么时候需要动它。
（`src/*.cs` 和 `android/**/*.java` 是源码，只做目录级说明，不逐条展开，见文末。）

---

## 1. 根目录

| 文件 | 大小 | 作用 | 什么时候会改 |
| --- | --- | --- | --- |
| `README.md` | 7.7 KB | 英文主门面：一句话简介、下载表、8 语言牌桌截图、阿拉伯语规则页与安装器截图、功能清单、一分钟规则、语言代码表、命令行参数、构建命令、目录树、Android 与仓库说明 | 加功能、发新版、换截图时 |
| `README.zh-CN.md` | 3.4 KB | 中文门面：与英文版同结构，面向中文读者，含「上传到 GitHub」的命令 | 同上 |
| `LICENSE` | 1.1 KB | MIT 许可全文，声明可自由使用/修改/分发 | 一般不动 |
| `CHANGELOG.md` | 2.2 KB | 版本变更记录，按 Added/Notes 分条；当前 `1.0.0`（8 语言、安装器、安卓） | 每次发版加一节 |
| `.gitignore` | 0.4 KB | 排除不该入库的东西：`build/out/`（临时构建）、`build/tools/`（JDK+Android SDK 工具链，几百 MB）、`selftest.txt`、`start.png` 等自检产物、`settings.ini`、`*.ini`、VS/Rider 缓存、`dist/*.idsig`（APK v4 签名附属文件） | 新增构建产物或本地缓存目录时 |
| `.gitattributes` | 0.2 KB | 声明文本文件正常换行处理，图片/exe/apk/zip 按二进制处理，避免 Windows 上被误转换 | 新增文件类型时 |

> `dist/` 里的四个成品**是特意入库的**：克隆下来就能直接玩，无需自己编译。

---

## 2. `.github/`

| 文件 | 大小 | 作用 | 什么时候会改 |
| --- | --- | --- | --- |
| `ci-build.yml` | 2.1 KB | GitHub Actions 两条流水线。`desktop`：Windows runner 上跑 `package.ps1`，用 `-selftest` 冒烟测试后上传 exe/zip/setup 三个 artifact；`android`：装 Temurin 17 + Android cmdline-tools，装 `platforms;android-33` 与 `build-tools;33.0.2`，再跑 `build-apk.ps1`，上传 APK | 改构建方式、升 SDK/Java 版本、加发布步骤时 |

---

## 3. `build/`（桌面构建与安装器）

| 文件 | 大小 | 作用 | 什么时候会改 |
| --- | --- | --- | --- |
| `build.ps1` | 1.5 KB | 只编译游戏本身：调 `csc.exe`（`/target:winexe /codepage:65001 /win32icon`，源码按 Core→Lang→Cards2D→UI→Game 顺序）输出 `dist\ZhengShangYou.exe` | 加源文件、换图标、改编译参数时 |
| `package.ps1` | 4.4 KB | 一键出三件套：先编译 exe，再打便携 zip（内含 `portable.txt`、8 语言 `README.txt`、`LICENSE.txt`），最后编译安装器并用 `"/resource:$exe,PAYLOAD"` 把游戏内嵌进去 | 改便携包内容、改安装器参数时 |
| `installer-Installer.cs` | 22.3 KB | 安装器 + 卸载器源码（WinForms，8 语言）。内含 `I18n` 语言表（8 × 20 条）、安装逻辑（写 exe、自复制为 `Uninstall.exe`、桌面/开始菜单快捷方式、HKCU 卸载项）、卸载逻辑（删快捷方式与注册表、隐藏 cmd 延时删目录），以及 `--silent` / `--dir=` / `--lang=` 参数 | 改安装路径、快捷方式、语言文案、卸载行为时 |

安装器的隐藏参数（便于自动化验证，日常用不到）：`--silent` 静默安装、`--uninstall` 静默卸载、`--dir=<路径>` 指定安装目录、`--lang=<代码>` 指定界面语言。

---

## 4. `docs/`（文档）

| 文件 | 大小 | 作用 | 什么时候会改 |
| --- | --- | --- | --- |
| `RULES.md` | 3.6 KB | 完整英文规则书：牌力 15 档、牌型与比较、首局红桃 4、上贡与要牌两步、30 秒计时、名次与 5 局流程、示例 | 规则调整时（同时要改 `Lang.cs` 的规则正文） |
| `BUILD.md` | 2.5 KB | 桌面构建说明：csc 单命令、产物清单、源码职责与编译顺序、自检命令、语言处理（`Lang.T`/`Lang.F`、RTL、自动缩字号）、安装器行为 | 改构建脚本或语言机制时 |
| `ANDROID.md` | 4.4 KB | 安卓构建说明：无 Gradle 的手工链、环境要求（JDK 17 + SDK 33）、`build-apk.ps1` 六个步骤、语言 API 与三处切换入口、验证状态（含未真机走查的说明）、安装方式、改语言表要重跑生成器的约定 | 改安卓构建、加语言、换签名时 |
| `OVERVIEW.md` | 8 KB | **仓库总介绍**：项目是什么、规则速览、四个版本对比、8 语言覆盖范围与入口、技术实现思路、目录地图、构建与自检、已验证/未验证边界、版本与许可 | 项目定位、功能范围变化时 |
| `docs/FILES.md` | 本文 | 除源码外每个文件的用途、大小与改动时机 | 新增/删除文件时 |

---

## 5. `assets/`（图标与截图）

| 文件 | 大小 | 作用 |
| --- | --- | --- |
| `game.ico` | 1.2 KB | 程序图标（64×64，PNG-in-ICO）。桌面 exe、安装器都用它；安卓五档 mipmap 也由它缩出 |
| `screenshot-start-en.png` / `start-zh-CN.png` | 128 / 123 KB | 开始页截图（英文、简中），README 顶部主图 |
| `assets/screenshots/game-<lang>.png` | 各约 216–228 KB | 8 语言牌桌截图：`zh-CN zh-TW en fr ru ja es ar`，README 用两列四行表格展示 |
| `screenshot-rules-en.png` / `rules-ar.png` | 51 / 43 KB | 规则页截图，后者用来展示阿拉伯语 RTL 正文 |
| `screenshot-installer-en.png` / `installer-ja.png` / `installer-ru.png` / `installer-ar.png` | 13–17 KB | 安装器四种语言窗口截图，README 用它证明安装器也做 8 语言 + RTL |

截图都是程序 `-snapshot` 或安装器 `--shot` 直接渲染出来的，不是截屏拼接。

---

## 6. `android/`（安卓工程，不含 Java 源码）

| 文件 | 大小 | 作用 |
| --- | --- | --- |
| `AndroidManifest.xml` | 1.0 KB | 应用清单：包名 `com.zhengshengyou.app`、versionCode/Name、minSdk 21、targetSdk 33、`sensorLandscape`、全屏无标题、`supportsRtl`、启动 Activity、图标与名称引用 `@mipmap/ic_launcher` / `@string/app_name` |
| `strings.xml` | 0.1 KB | 默认（英文）应用名 `Zheng Shang You` |
| `strings-zh-rCN.xml` | 0.1 KB | 简中桌面图标名「争上游」 |
| `strings-zh-rTW.xml` | 0.1 KB | 繁中桌面图标名「爭上游」 |
| `strings-ja.xml` | 0.1 KB | 日文图标名「争上游」 |
| `android/app/src/main/res/values-fr/`, `values-ru/`, `values-es/`, `values-ar/` 下的 `strings.xml` | 各 0.1 KB | 法/俄/西/阿的图标名（保持品牌拉丁写法） |
| `android/app/src/main/res/mipmap-mdpi/`、`mipmap-hdpi/`、`mipmap-xhdpi/`、`mipmap-xxhdpi/`、`mipmap-xxxhdpi/` 下的 `ic_launcher.png` | 1.8–17.7 KB | 五档启动图标（48/72/96/144/192），由 `game.ico` 缩出 |
| `build-apk.ps1` | 5.6 KB | APK 全流程：`generate-lang.ps1` → JVM 自测 → `aapt2 compile/link` → `javac`（Java 8 字节码，`-bootclasspath android.jar`）→ `d8`（直接调 jar，绕开 build-tools 33 的 `-Djava.ext.dirs` 与 JDK 17 不兼容）→ 把 `classes.dex` 塞进 APK → `zipalign -f 4` → `keytool` 生成调试 keystore → `apksigner sign/verify`，输出 `dist\ZhengShangYou.apk` |
| `generate-lang.ps1` | 5.2 KB | 从 `Lang.cs` 生成 `Lang.java` 的生成器：逐字符扫描（能正确处理文案里的 `}` 和转义），输出 8 × 108 条字符串与 `setCode/t/code/idx/isRtl/f` API。**改文案只改 `Lang.cs` 后重跑它，别手改 `Lang.java`** |

`android/app/src/main/java/` 下是安卓源码，不在此逐条说明：

* `com/zhengshengyou/engine/`：纯 Java 规则引擎（`Card` 牌与牌力、`Play` 一手牌、`PlayRules` 解析与比较、`Game` 发牌/换牌状态机/AI、`Lang` 生成的语言表、`SelfTest` 300 局断言）。
* `com/zhengshengyou/app/`：`MainActivity`（语言检测与持久化、沉浸式全屏、返回键退出确认）与 `TableView`（横屏触屏牌桌：顶栏语言按钮、两侧对手面板、中央出牌区、日志、手牌重叠排列与点选、四个弹窗、计时与 AI 调度）。

---

## 7. `dist/`（成品，直接可运行）

| 文件 | 大小 | 作用 | 什么时候重新生成 |
| --- | --- | --- | --- |
| `ZhengShangYou.exe` | 129.5 KB | 桌面游戏本体，单文件、8 语言、真实牌面，双击即玩 | `build.ps1` 或 `package.ps1` |
| `ZhengShangYou-Portable.zip` | 51 KB | 便携版：`ZhengShangYou.exe` + `portable.txt`（设置写同目录）+ 8 语言 `README.txt` + `LICENSE.txt` | `package.ps1` |
| `ZhengShangYou-Setup.exe` | 156 KB | 8 语言安装器/卸载器，游戏 exe 内嵌其中，安装到 `%LOCALAPPDATA%\Programs\ZhengShangYou` | `package.ps1` |
| `ZhengShangYou.apk` | 92.8 KB | 安卓安装包，minSdk 21 / targetSdk 33，调试密钥签名，含 8 语言与本地化图标名 | `build-apk.ps1` |

---

## 8. 哪些文件**不入库**（由 `.gitignore` 管）

| 路径 | 原因 |
| --- | --- |
| `build/tools/` | 本地下载的 JDK 17 与 Android SDK（几百 MB），CI 会自己装 |
| `build/out/` | 编译中间产物、测试安装目录、安装器截图临时目录 |
| `selftest.txt`、`start.png`、`game.png`、`dialog.png`、`rank.png`、`rules.png`、`lang.png` | 运行游戏自检时生成，随时可重跑 |
| `settings.ini` / `*.ini` | 用户个人的语言与偏好设置 |
| `dist/*.idsig` | apksigner 的 v4 签名附属文件（APK 本身已入库） |
| `.vs/`、`.idea/`、`*.user`、`*.suo` | 编辑器缓存 |

---

## 9. 命名与体积约定

* 所有**分发文件**（exe / zip / apk / 截图 / 源码 / 脚本）一律英文命名，避免链接与命令行乱码；仅仓库外层的「上传包」和中文 README 用中文名。
* `dist/` 的成品刻意做小：桌面 exe 约 130 KB，APK 约 93 KB，整包不到 1 MB，方便直接放进仓库或邮件发送。
* 文案改动只有一个入口：`Lang.cs`。桌面直接编译，安卓由 `generate-lang.ps1` 生成；两端的 key 数量与内容由脚本保证一致。
