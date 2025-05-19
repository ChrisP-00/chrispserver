# [ Master DB ]
  
## Database Creation

```mssql
IF DB_ID(N'masterdb') IS NOT NULL
BEGIN
    ALTER DATABASE masterdb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE masterdb;
END

CREATE DATABASE masterdb;
```

## Define Table 

```mssql
IF OBJECT_ID('info_define', 'U') IS NOT NULL DROP TABLE info_define;
CREATE TABLE info_define (
    define_index INT NOT NULL PRIMARY KEY,         -- ���� ���� �ĺ���
    description NVARCHAR(20) NULL,                 -- ����
    value FLOAT NOT NULL DEFAULT 0                 -- ��
);

EXEC sp_addextendedproperty N'MS_Description', N'�⺻ ���� ���̺�', 'SCHEMA', 'dbo', 'TABLE', 'info_define';
EXEC sp_addextendedproperty N'MS_Description', N'���� ���� �ĺ���', 'SCHEMA', 'dbo', 'TABLE', 'info_define', 'COLUMN', 'define_index';
EXEC sp_addextendedproperty N'MS_Description', N'����', 'SCHEMA', 'dbo', 'TABLE', 'info_define', 'COLUMN', 'description';
EXEC sp_addextendedproperty N'MS_Description', N'��', 'SCHEMA', 'dbo', 'TABLE', 'info_define', 'COLUMN', 'value';
```

## Character Table
```mssql
IF OBJECT_ID('info_character', 'U') IS NOT NULL DROP TABLE info_character;
CREATE TABLE info_character (
    character_index INT NOT NULL PRIMARY KEY,      -- ĳ���� ���� �ĺ���
    character_name NVARCHAR(10) NULL               -- ĳ���� �̸� (������� ����)
);

EXEC sp_addextendedproperty N'MS_Description', N'���� �� ��� ĳ������ �⺻ ���� ���̺�', 'SCHEMA', 'dbo', 'TABLE', 'info_character';
EXEC sp_addextendedproperty N'MS_Description', N'ĳ���� ���� �ĺ���', 'SCHEMA', 'dbo', 'TABLE', 'info_character', 'COLUMN', 'character_index';
EXEC sp_addextendedproperty N'MS_Description', N'ĳ���� �̸� (������� ����)', 'SCHEMA', 'dbo', 'TABLE', 'info_character', 'COLUMN', 'character_name';
```

## Daily_Mission Table
```mssql
IF OBJECT_ID('info_daily_mission', 'U') IS NOT NULL DROP TABLE info_daily_mission;
CREATE TABLE info_daily_mission (
    daily_mission_index INT NOT NULL PRIMARY KEY,  -- ���� �̼� ���� �ĺ���
    goods_type INT NOT NULL,                       -- ����ؾ� �ϴ� ��ȭ Ÿ��
    goods_index INT NOT NULL,                      -- ����ؾ� �ϴ� ��ȭ �ε���
    mission_goal_count TINYINT NOT NULL,           -- �̼� ���� ��ǥ ��
    reward_type TINYINT NOT NULL,                  -- ���� Ÿ�� (goods_index ����)
    reward_amount TINYINT NOT NULL                 -- ���� ����
);

EXEC sp_addextendedproperty N'MS_Description', N'���� �̼� ���� ���̺�', 'SCHEMA', 'dbo', 'TABLE', 'info_daily_mission';
EXEC sp_addextendedproperty N'MS_Description', N'���� �̼� ���� �ĺ���', 'SCHEMA', 'dbo', 'TABLE', 'info_daily_mission', 'COLUMN', 'daily_mission_index';
EXEC sp_addextendedproperty N'MS_Description', N'����ؾ� �ϴ� ��ȭ Ÿ��', 'SCHEMA', 'dbo', 'TABLE', 'info_daily_mission', 'COLUMN', 'goods_type';
EXEC sp_addextendedproperty N'MS_Description', N'����ؾ� �ϴ� ��ȭ �ε���', 'SCHEMA', 'dbo', 'TABLE', 'info_daily_mission', 'COLUMN', 'goods_index';
EXEC sp_addextendedproperty N'MS_Description', N'�̼� ���� ��ǥ ��', 'SCHEMA', 'dbo', 'TABLE', 'info_daily_mission', 'COLUMN', 'mission_goal_count';
EXEC sp_addextendedproperty N'MS_Description', N'���� Ÿ�� (goods_index ����)', 'SCHEMA', 'dbo', 'TABLE', 'info_daily_mission', 'COLUMN', 'reward_type';
EXEC sp_addextendedproperty N'MS_Description', N'���� ����', 'SCHEMA', 'dbo', 'TABLE', 'info_daily_mission', 'COLUMN', 'reward_amount';
```

## Goods Table

```mssql
IF OBJECT_ID('info_goods', 'U') IS NOT NULL DROP TABLE info_goods;
CREATE TABLE info_goods (
    goods_index INT NOT NULL PRIMARY KEY,          -- ��ȭ ���� �ĺ���
    goods_type INT NOT NULL,                       -- ��ȭ ���� (1 = ����, 2 = �峭��, 3 = �̱�, 4 = ����Ʈ ���)
    goods_name NVARCHAR(20) NULL                   -- ��ȭ �̸�
);

EXEC sp_addextendedproperty N'MS_Description', N'��� ��ȭ�� ���� ���� ���̺�', 'SCHEMA', 'dbo', 'TABLE', 'info_goods';
EXEC sp_addextendedproperty N'MS_Description', N'��ȭ ���� �ĺ���', 'SCHEMA', 'dbo', 'TABLE', 'info_goods', 'COLUMN', 'goods_index';
EXEC sp_addextendedproperty N'MS_Description', N'��ȭ ���� (1 = ����, 2 = �峭��, 3 = �̱�, 4 = ����Ʈ ���)', 'SCHEMA', 'dbo', 'TABLE', 'info_goods', 'COLUMN', 'goods_type';
EXEC sp_addextendedproperty N'MS_Description', N'��ȭ �̸�', 'SCHEMA', 'dbo', 'TABLE', 'info_goods', 'COLUMN', 'goods_name';
```

## Item Table
```mssql
IF OBJECT_ID('info_item', 'U') IS NOT NULL DROP TABLE info_item;
CREATE TABLE info_item (
    item_index INT NOT NULL PRIMARY KEY,           -- ��ǰ ���� �ĺ���
    item_name NVARCHAR(100) NULL,                  -- ��ǰ �̸�
    item_type TINYINT NOT NULL,                    -- ������ Ÿ�� (���� ���� ��)
    required_level INT NOT NULL DEFAULT 0,         -- ��� ������ �ּ� ĳ���� ����
    equip_character_index INT NOT NULL             -- ���� ������ ĳ���� �ε���
);

EXEC sp_addextendedproperty N'MS_Description', N'���� �� ��� ������ �⺻ ���� ���̺�', 'SCHEMA', 'dbo', 'TABLE', 'info_item';
EXEC sp_addextendedproperty N'MS_Description', N'��ǰ ���� �ĺ���', 'SCHEMA', 'dbo', 'TABLE', 'info_item', 'COLUMN', 'item_index';
EXEC sp_addextendedproperty N'MS_Description', N'��ǰ �̸�', 'SCHEMA', 'dbo', 'TABLE', 'info_item', 'COLUMN', 'item_name';
EXEC sp_addextendedproperty N'MS_Description', N'������ Ÿ�� (���� ���� ��)', 'SCHEMA', 'dbo', 'TABLE', 'info_item', 'COLUMN', 'item_type';
EXEC sp_addextendedproperty N'MS_Description', N'��� ������ �ּ� ĳ���� ����', 'SCHEMA', 'dbo', 'TABLE', 'info_item', 'COLUMN', 'required_level';
EXEC sp_addextendedproperty N'MS_Description', N'���� ������ ĳ���� �ε���', 'SCHEMA', 'dbo', 'TABLE', 'info_item', 'COLUMN', 'equip_character_index';
```

## LevelUp Table
```mssql
IF OBJECT_ID('info_levels', 'U') IS NOT NULL DROP TABLE info_levels;
CREATE TABLE info_levels (
    level_index INT NOT NULL PRIMARY KEY,          -- ���� �� ���� �ĺ���
    character_index INT NOT NULL,                  -- ĳ���� ���� �ĺ���
    level INT NOT NULL DEFAULT 0,                  -- ĳ���� ����
    required_exp INT NOT NULL DEFAULT 0            -- ĳ���� ����ġ
);

EXEC sp_addextendedproperty N'MS_Description', N'ĳ���� ���� �� ���� ���̺�', 'SCHEMA', 'dbo', 'TABLE', 'info_levels';
EXEC sp_addextendedproperty N'MS_Description', N'���� �� ���� �ĺ���', 'SCHEMA', 'dbo', 'TABLE', 'info_levels', 'COLUMN', 'level_index';
EXEC sp_addextendedproperty N'MS_Description', N'ĳ���� ���� �ĺ���', 'SCHEMA', 'dbo', 'TABLE', 'info_levels', 'COLUMN', 'character_index';
EXEC sp_addextendedproperty N'MS_Description', N'ĳ���� ����', 'SCHEMA', 'dbo', 'TABLE', 'info_levels', 'COLUMN', 'level';
EXEC sp_addextendedproperty N'MS_Description', N'ĳ���� ����ġ', 'SCHEMA', 'dbo', 'TABLE', 'info_levels', 'COLUMN', 'required_exp';
```

## Product Table
```mssql
IF OBJECT_ID('info_products', 'U') IS NOT NULL DROP TABLE info_products;
CREATE TABLE info_products (
    product_index INT NOT NULL PRIMARY KEY,        -- ĳ���� ��ǰ ���� �ĺ���
    product_name NVARCHAR(10) NULL                 -- ĳ���� ��ǰ �̸� (������� ����)
);

EXEC sp_addextendedproperty N'MS_Description', N'���� �� ��� ĳ���� ��ǰ �⺻ ���� ���̺�', 'SCHEMA', 'dbo', 'TABLE', 'info_products';
EXEC sp_addextendedproperty N'MS_Description', N'ĳ���� ��ǰ ���� �ĺ���', 'SCHEMA', 'dbo', 'TABLE', 'info_products', 'COLUMN', 'product_index';
EXEC sp_addextendedproperty N'MS_Description', N'ĳ���� ��ǰ �̸� (������� ����)', 'SCHEMA', 'dbo', 'TABLE', 'info_products', 'COLUMN', 'product_name';
```