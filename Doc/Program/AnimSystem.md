﻿# 动画系统

3-27

播放完后恢复原状，不造成状态变化的使用Feedback，如撞击动画，受击动画（也就是所有模型动画，这部分相当于状态机动画+特效），镜头晃动等大部分动画，

播放完会影响状态使用DoTween，即移动位置，旋转等和游戏逻辑相关的。