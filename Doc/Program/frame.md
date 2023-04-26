# 框架

1. 先实现完整的游戏模拟
2. 实现序列化，实现复制
3. 考虑实现命令。

### 实例化

Unit/Tile？
1. 分为Data，Model，Render
   1. Data
      1. So可以创建Data
      2. Data只读，唯一
   2. Model
      1. Model用Data初始化
      2. Model包含和维护运行时信息
      3. Model层完成游戏逻辑模拟
   3. Render
      1. 订阅Model事件
      2. 生成动画


### Unit
1. UnitScript
2. passable
3. 
### Tile
## 碰撞

## 回合

<!-- 1. 回合开始
2. 玩家1先操作
3. 攻击并结算
4. 尝试移动，按控制器序号顺序处理
   1. 处理碰撞
      1. 移动者（有碰撞）遍历到达块的LastUnits（有碰撞），如果LastUnit在原地或往移动者方向运动视为碰撞
   2. 忽略其他可移动实体，
5. 回合结束 -->

1. 有玩家和发射实体（触碰即消失）
2. 发射实体只需要固定控制器，不需要ai
3. 


## 事件
### EventPool
'''
public event Action<GridUnit, GridUnitData> ResetData;
public event Action<GridUnit, GridUnit> UnitIn, UnitStay, UnitOut;
public event Action<GridUnit, GridTile> LeaveTile, StayTile, EnterTile;
public event Action<GridUnit, Direction, GridUnit> Collide;
public event Action<GridUnit> Update, LateUpdate, Die;
public event Action<GridUnit, int> HealthChange;
'''
分为发出者和接收者
### Tile

1. 实体到达（Entity）
2. 实体离开（Entity）

### Unit

1. 实体可越过/不可越过

2. 不可越过-》碰撞（方向，Entity）
3. 可越过-》实体到达（Entity）
4. 可越过-》实体离开（Entity）
5. 破坏（）

### Entity：Unit

1. 控制器

2. 移动（）
3. 移动到（Unit）

### Player：Entity

1. 拾取（）

### Item——》Entity
