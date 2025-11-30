# 音乐管理应用 - DailyMusicA

这是一个功能完善的音乐管理应用程序，使用Avalonia UI框架开发，采用MVVM架构模式，支持专辑管理、歌曲收藏、数据统计等多项功能。应用支持跨平台运行，包括Windows、Linux和macOS。

## 📋 目录

- [技术栈](#技术栈)
- [项目结构](#项目结构)
- [功能特性](#功能特性)
- [安装与运行](#安装与运行)
- [主要功能模块](#主要功能模块)
  - [专辑管理](#专辑管理)
  - [歌曲收藏](#歌曲收藏)
  - [数据统计](#数据统计)
  - [用户体验优化](#用户体验优化)
- [使用说明](#使用说明)
- [开发指南](#开发指南)
- [已知问题](#已知问题)

## 🛠 技术栈

- **UI框架**: Avalonia UI
- **MVVM框架**: CommunityToolkit.Mvvm
- **数据库**: SQLite
- **依赖注入**: Microsoft.Extensions.DependencyInjection
- **开发语言**: C#
- **构建工具**: .NET

## 📁 项目结构

```
├── DailyMusicA.Library/         # 核心库
│   ├── Models/                  # 数据模型（Album, Song等）
│   ├── Services/                # 服务层（数据存储、文件操作等）
│   ├── ViewModels/              # 视图模型（业务逻辑）
│   └── Helpers/                 # 工具类和辅助方法
├── DailyMusicA/                 # 应用程序
│   ├── Views/                   # 用户界面
│   ├── Services/                # 应用服务
│   ├── Converters/              # 值转换器
│   └── Assets/                  # 资源文件
├── DailyMusicA.UnitTest/        # 单元测试
└── TestMusicDbProject/          # 数据库测试工具
```

## ✨ 功能特性

### 专辑管理
- 添加、编辑、删除专辑
- 专辑封面上传与自动删除
- 专辑详情查看
- 专辑列表分页显示

### 歌曲收藏
- 收藏/取消收藏歌曲
- 收藏歌曲列表管理
- 按专辑查看歌曲

### 数据统计
- 应用统计概览（专辑数、歌曲数等）
- 收藏统计
- 专辑发行年份统计

### 用户体验优化
- 添加专辑后自动清空封面
- 编辑专辑后自动返回
- 删除专辑时自动清理相关文件
- 响应式布局设计

## 🚀 安装与运行

### 系统要求
- .NET 6.0 或更高版本
- 支持的操作系统：Windows 7+, macOS 10.13+, Ubuntu 18.04+

### 构建与运行

```bash
# 克隆仓库（如果需要）
git clone <repository-url>
cd testgit-master3

# 还原依赖
dotnet restore

# 构建项目
dotnet build

# 运行应用
dotnet run --project DailyMusicA
```

## 🔍 主要功能模块

### 专辑管理

#### 专辑列表 (AlbumView & AlbumViewModel)
- 显示所有专辑
- 支持搜索和过滤
- 提供添加、编辑、删除操作

#### 专辑详情 (AlbumDetailView & AlbumDetailViewModel)
- 显示专辑完整信息
- 支持修改专辑信息
- 保存后自动返回上一页
- 专辑封面管理

#### 添加专辑 (AddAlbumView & AddAlbumViewModel)
- 专辑信息表单
- 封面图片上传
- 添加后自动清空封面选择

### 歌曲收藏

#### 收藏管理 (FavoriteView & FavoriteViewModel)
- 显示所有收藏的歌曲
- 支持取消收藏
- 按专辑分组显示

### 数据统计

#### 统计概览 (HomeView & HomeViewModel)
- 显示关键统计数据
- 提供快捷导航

#### 详细统计 (StatsView & StatsViewModel)
- 专辑数量统计
- 歌曲数量统计
- 收藏统计图表

## 📖 使用说明

### 添加新专辑
1. 从侧边栏导航到「专辑」页面
2. 点击「添加专辑」按钮
3. 填写专辑信息
4. 点击「选择封面」上传专辑封面
5. 点击「保存」完成添加（封面会自动清空）

### 编辑专辑
1. 在专辑列表中选择要编辑的专辑
2. 点击「编辑」按钮
3. 修改专辑信息
4. 点击「保存」按钮（系统会自动返回专辑列表）

### 删除专辑
1. 在专辑列表中选择要删除的专辑
2. 点击「删除」按钮
3. 确认删除操作（相关封面文件会自动清理）

### 收藏歌曲
1. 在专辑详情页面，找到要收藏的歌曲
2. 点击歌曲旁的「收藏」按钮
3. 歌曲将出现在「我的收藏」页面

## 👨‍💻 开发指南

### 数据模型
主要数据模型包括：
- `Album`: 专辑信息
- `Song`: 歌曲信息
- `Favorite`: 收藏关系

### 服务层
核心服务：
- `IMusicStorage`: 数据存储接口，提供专辑和歌曲的CRUD操作
- `IFileStorage`: 文件存储接口，处理封面图片的保存和删除
- `IMenuNavigationService`: 导航服务，管理页面切换

### MVVM实现
- 使用CommunityToolkit.Mvvm实现MVVM模式
- 视图模型继承自`ViewModelBase`
- 使用`RelayCommand`实现命令绑定
- 属性变更通知通过`ObservableObject`实现

## ⚠️ 已知问题
- 部分平台可能存在UI渲染差异
- 大量专辑数据时可能需要优化性能
- 封面图片大小限制为10MB

## 📝 版本历史

### 最新版本
- 添加专辑后自动清空封面选择
- 编辑专辑后自动返回
- 删除专辑时自动清理封面文件
- 优化用户体验

---

*感谢使用DailyMusicA音乐管理应用！如有问题或建议，请提交issue。*