1.基本设计：
PeCmpt的子类尽量只做game play的功能模块，策划要的逻辑模块，比如开车，招募，背包等，
其他要用到mono的FixedUpdate，LateUpdate之类的模块，尽量写成还是继承自mono。

2.模块说明，如果有合适的命名，请提出来讨论修改。
1）ActivationCmpt		在接收到激活和不激活信息的时候正确处理entity的激活状况。
2）AnimCmpt				animator和animation的统一接口，其他模块要播放动画等，引用它。
3）AnimCmpt_Animator	animator版的AnimCmpt实现。
4）AttributeCmpt		联系SkEntity和pecmpt
5）AvatarCmpt			模型销毁和重建
6）CarriorCmpt			上下，操纵玩家制造的載具。motion下有个Motion_Drive可以考虑合并。
7）DetectorCmpt			探测器模块，模拟感觉器官。
8）EquipmentCmpt		装备模块
9）FadeInOutCmpt		渐隐消失
10）FollowCmpt			跟随，不一定需要。
11）HeadInfoCmpt		头顶信息显示
12）HumanIK				ik模块
13）PackageCmpt			包裹
14）PeTrans				主要用于获取位置信息。
15）TrainCmpt			上轻轨模块
16）ViewCmpt			维护模型身体，肤色等。
17）CompsiteAction		组合行为，比如一个npc背另一个，开会等，需要多个entity配合的行为。
18）CompsiteAction/PackageCmpt	一个npc背另一个
19）Motion				需要互斥资源的模块
20）MotionBaseCmpt		具有互斥关系的模块继承此类。
21）Motion_Drive		驾驶載具
22）MotionMgrCmpt		维护当前互斥关系，供motion查询。
23）Motion_Move			统一的移动模块，提供移动到某点，朝哪儿移动等。