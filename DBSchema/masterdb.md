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
    define_index INT NOT NULL PRIMARY KEY,         -- 정의 고유 식별자
    description NVARCHAR(20) NULL,                 -- 설명
    value FLOAT NOT NULL DEFAULT 0                 -- 값
);

EXEC sp_addextendedproperty N'MS_Description', N'기본 정의 테이블', 'SCHEMA', 'dbo', 'TABLE', 'info_define';
EXEC sp_addextendedproperty N'MS_Description', N'정의 고유 식별자', 'SCHEMA', 'dbo', 'TABLE', 'info_define', 'COLUMN', 'define_index';
EXEC sp_addextendedproperty N'MS_Description', N'설명', 'SCHEMA', 'dbo', 'TABLE', 'info_define', 'COLUMN', 'description';
EXEC sp_addextendedproperty N'MS_Description', N'값', 'SCHEMA', 'dbo', 'TABLE', 'info_define', 'COLUMN', 'value';
```

## Character Table
```mssql
IF OBJECT_ID('info_character', 'U') IS NOT NULL DROP TABLE info_character;
CREATE TABLE info_character (
    character_index INT NOT NULL PRIMARY KEY,      -- 캐릭터 고유 식별자
    character_name NVARCHAR(10) NULL               -- 캐릭터 이름 (변경되지 않음)
);

EXEC sp_addextendedproperty N'MS_Description', N'게임 내 모든 캐릭터의 기본 정보 테이블', 'SCHEMA', 'dbo', 'TABLE', 'info_character';
EXEC sp_addextendedproperty N'MS_Description', N'캐릭터 고유 식별자', 'SCHEMA', 'dbo', 'TABLE', 'info_character', 'COLUMN', 'character_index';
EXEC sp_addextendedproperty N'MS_Description', N'캐릭터 이름 (변경되지 않음)', 'SCHEMA', 'dbo', 'TABLE', 'info_character', 'COLUMN', 'character_name';
```

## Daily_Mission Table
```mssql
IF OBJECT_ID('info_daily_mission', 'U') IS NOT NULL DROP TABLE info_daily_mission;
CREATE TABLE info_daily_mission (
    daily_mission_index INT NOT NULL PRIMARY KEY,  -- 일일 미션 고유 식별자
    goods_type INT NOT NULL,                       -- 사용해야 하는 재화 타입
    goods_index INT NOT NULL,                      -- 사용해야 하는 재화 인덱스
    mission_goal_count TINYINT NOT NULL,           -- 미션 수행 목표 수
    reward_type TINYINT NOT NULL,                  -- 보상 타입 (goods_index 참조)
    reward_amount TINYINT NOT NULL                 -- 보상 수량
);

EXEC sp_addextendedproperty N'MS_Description', N'일일 미션 정보 테이블', 'SCHEMA', 'dbo', 'TABLE', 'info_daily_mission';
EXEC sp_addextendedproperty N'MS_Description', N'일일 미션 고유 식별자', 'SCHEMA', 'dbo', 'TABLE', 'info_daily_mission', 'COLUMN', 'daily_mission_index';
EXEC sp_addextendedproperty N'MS_Description', N'사용해야 하는 재화 타입', 'SCHEMA', 'dbo', 'TABLE', 'info_daily_mission', 'COLUMN', 'goods_type';
EXEC sp_addextendedproperty N'MS_Description', N'사용해야 하는 재화 인덱스', 'SCHEMA', 'dbo', 'TABLE', 'info_daily_mission', 'COLUMN', 'goods_index';
EXEC sp_addextendedproperty N'MS_Description', N'미션 수행 목표 수', 'SCHEMA', 'dbo', 'TABLE', 'info_daily_mission', 'COLUMN', 'mission_goal_count';
EXEC sp_addextendedproperty N'MS_Description', N'보상 타입 (goods_index 참조)', 'SCHEMA', 'dbo', 'TABLE', 'info_daily_mission', 'COLUMN', 'reward_type';
EXEC sp_addextendedproperty N'MS_Description', N'보상 수량', 'SCHEMA', 'dbo', 'TABLE', 'info_daily_mission', 'COLUMN', 'reward_amount';
```

## Goods Table

```mssql
IF OBJECT_ID('info_goods', 'U') IS NOT NULL DROP TABLE info_goods;
CREATE TABLE info_goods (
    goods_index INT NOT NULL PRIMARY KEY,          -- 재화 고유 식별자
    goods_type INT NOT NULL,                       -- 재화 종류 (1 = 간식, 2 = 장난감, 3 = 뽑기, 4 = 포인트 등등)
    goods_name NVARCHAR(20) NULL                   -- 재화 이름
);

EXEC sp_addextendedproperty N'MS_Description', N'모든 재화에 대한 정보 테이블', 'SCHEMA', 'dbo', 'TABLE', 'info_goods';
EXEC sp_addextendedproperty N'MS_Description', N'재화 고유 식별자', 'SCHEMA', 'dbo', 'TABLE', 'info_goods', 'COLUMN', 'goods_index';
EXEC sp_addextendedproperty N'MS_Description', N'재화 종류 (1 = 간식, 2 = 장난감, 3 = 뽑기, 4 = 포인트 등등)', 'SCHEMA', 'dbo', 'TABLE', 'info_goods', 'COLUMN', 'goods_type';
EXEC sp_addextendedproperty N'MS_Description', N'재화 이름', 'SCHEMA', 'dbo', 'TABLE', 'info_goods', 'COLUMN', 'goods_name';
```

## Item Table
```mssql
IF OBJECT_ID('info_item', 'U') IS NOT NULL DROP TABLE info_item;
CREATE TABLE info_item (
    item_index INT NOT NULL PRIMARY KEY,           -- 소품 고유 식별자
    item_name NVARCHAR(100) NULL,                  -- 소품 이름
    item_type TINYINT NOT NULL,                    -- 아이템 타입 (장착 부위 등)
    required_level INT NOT NULL DEFAULT 0,         -- 사용 가능한 최소 캐릭터 레벨
    equip_character_index INT NOT NULL             -- 장착 가능한 캐릭터 인덱스
);

EXEC sp_addextendedproperty N'MS_Description', N'게임 내 모든 아이템 기본 정보 테이블', 'SCHEMA', 'dbo', 'TABLE', 'info_item';
EXEC sp_addextendedproperty N'MS_Description', N'소품 고유 식별자', 'SCHEMA', 'dbo', 'TABLE', 'info_item', 'COLUMN', 'item_index';
EXEC sp_addextendedproperty N'MS_Description', N'소품 이름', 'SCHEMA', 'dbo', 'TABLE', 'info_item', 'COLUMN', 'item_name';
EXEC sp_addextendedproperty N'MS_Description', N'아이템 타입 (장착 부위 등)', 'SCHEMA', 'dbo', 'TABLE', 'info_item', 'COLUMN', 'item_type';
EXEC sp_addextendedproperty N'MS_Description', N'사용 가능한 최소 캐릭터 레벨', 'SCHEMA', 'dbo', 'TABLE', 'info_item', 'COLUMN', 'required_level';
EXEC sp_addextendedproperty N'MS_Description', N'장착 가능한 캐릭터 인덱스', 'SCHEMA', 'dbo', 'TABLE', 'info_item', 'COLUMN', 'equip_character_index';
```

## LevelUp Table
```mssql
IF OBJECT_ID('info_levels', 'U') IS NOT NULL DROP TABLE info_levels;
CREATE TABLE info_levels (
    level_index INT NOT NULL PRIMARY KEY,          -- 레벨 업 고유 식별자
    character_index INT NOT NULL,                  -- 캐릭터 고유 식별자
    level INT NOT NULL DEFAULT 0,                  -- 캐릭터 레벨
    required_exp INT NOT NULL DEFAULT 0            -- 캐릭터 경험치
);

EXEC sp_addextendedproperty N'MS_Description', N'캐릭터 레벨 업 정보 테이블', 'SCHEMA', 'dbo', 'TABLE', 'info_levels';
EXEC sp_addextendedproperty N'MS_Description', N'레벨 업 고유 식별자', 'SCHEMA', 'dbo', 'TABLE', 'info_levels', 'COLUMN', 'level_index';
EXEC sp_addextendedproperty N'MS_Description', N'캐릭터 고유 식별자', 'SCHEMA', 'dbo', 'TABLE', 'info_levels', 'COLUMN', 'character_index';
EXEC sp_addextendedproperty N'MS_Description', N'캐릭터 레벨', 'SCHEMA', 'dbo', 'TABLE', 'info_levels', 'COLUMN', 'level';
EXEC sp_addextendedproperty N'MS_Description', N'캐릭터 경험치', 'SCHEMA', 'dbo', 'TABLE', 'info_levels', 'COLUMN', 'required_exp';
```

## Product Table
```mssql
IF OBJECT_ID('info_products', 'U') IS NOT NULL DROP TABLE info_products;
CREATE TABLE info_products (
    product_index INT NOT NULL PRIMARY KEY,        -- 캐릭터 제품 고유 식별자
    product_name NVARCHAR(10) NULL                 -- 캐릭터 제품 이름 (변경되지 않음)
);

EXEC sp_addextendedproperty N'MS_Description', N'게임 내 모든 캐릭터 제품 기본 정보 테이블', 'SCHEMA', 'dbo', 'TABLE', 'info_products';
EXEC sp_addextendedproperty N'MS_Description', N'캐릭터 제품 고유 식별자', 'SCHEMA', 'dbo', 'TABLE', 'info_products', 'COLUMN', 'product_index';
EXEC sp_addextendedproperty N'MS_Description', N'캐릭터 제품 이름 (변경되지 않음)', 'SCHEMA', 'dbo', 'TABLE', 'info_products', 'COLUMN', 'product_name';
```