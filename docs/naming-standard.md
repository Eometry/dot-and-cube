# 命名规范 Naming Standard

## 一、缩写 Abbreviation
缩写通常适用于复合单词的变量或对象命名；对于常用且长度较短的单个单词，优先考虑使用完整单词。

### 1. 对象名称 Objects
| Abbr. | Full Term | 含义 |
| :--- | :--- | :--- |
| `ply` | player | 玩家 |

### 2. 变量核心名词 Core Nouns
| Abbr. | Full Term | 含义 |
| :--- | :--- | :--- |
| `dir` | direction | 方向/方向向量 |
| `dist` | distance | 距离 |
| `idx` | index | 索引 |
| `pos` | position | 空间位置 |
| `seg` | segment | 线段 |

### 3. 修饰词 Modifiers
| Abbr. | Full Term | 含义 |
| :--- | :--- | :--- |
| `min` | minimum | 最小值 |
| `max` | maximum | 最大值 |
| `neg` | negative | 取反/负值 |
| `abs` | absolute | 绝对值 |
| `inv` | inverse | 倒数 |
| `log` | logarithm | 对数 |
| `ln` | natural logarithm | 自然对数 |
| `exp` | exponential | 指数 |
| `acc` | accumulated | 累计量 |

### 4. 单位与量纲 Units & Dimensions
| Abbr. | Full Term | 含义 |
| :--- | :--- | :--- |
| `deg` | degree | 角度制 |
| `rad` | radian | 弧度制 |
| `px` | pixel | 像素 |
| `db` | decibel | 分贝 |

## 二、资源前缀 Resource Prefixes

### 1. 音频资源 Audio Resources
| Abbr. | Full Term | 含义 |
| :--- | :--- | :--- |
| `mus` | music | 游戏音乐 |
| `snd` | sound | 音效 |
| `vox` | voice | 语音音效 |

## 三、字符大小写规范 Casing Conventions

| Object | Casing | Example |
| :--- | :--- | :--- |
| 类 | `PascalCase` | `class DotBase` |
| 方法 | `PascalCase` | `void UpdateSegIdx()` |
| Godot 内置虚方法 | `_PascalCase` | `override void _Ready()` |
| 属性 | `PascalCase` | `[Export] public float Speed` |
| 局部变量 | `camelCase` | `int currentSegIdx` |
| 参数 | `camelCase` | `double delta` |
| 私有字段 | `_camelCase` | `private float[] _accTimes` |
| 常量 | `UPPER_CASE` | `const float INV_LN2` |
| 静态只读对象 | `UPPER_CASE` | `static readonly Color TRANSPARENT` |
| 文件夹 | `snake_case` | `scripts/` |
| 场景文件 | `snake_case` | `test_room.tscn` |
| 资源文件 | `snake_case` | `snd_explosion.wav` |
