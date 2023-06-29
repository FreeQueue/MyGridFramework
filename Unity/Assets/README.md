# Survive 100 (0.3)

## Description

## 结构

### 1. Framework

#### Base

1. Rules
   提供框架的约定类，框架的使用者应该尽量使用约定类，作为合作的基础。
   如IUnRegister约定。
2. Utils
   提供框架的工具类，框架的使用者尽量使用工具类，而不是自建工具类，避免额外的缓存等消耗。

#### Kits

各种工具包，如UI，网络，数据，音频等。
工具包可以依赖于Base，和其他工具包，工具包不能依赖于框架入口和模块。
工具包要尽量自成一体。
工具包也可以是插件或插件的封装。

#### Core

框架入口，提供框架的初始化，更新，销毁等功能。
以及IModule，自建模块的基类。

### Game

## Coding Style