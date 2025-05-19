# [ GameServer DB ]
  
## Database Creation

```mssql
IF DB_ID(N'gameserverdb') IS NOT NULL
BEGIN
    ALTER DATABASE gameserverdb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE gameserverdb;
END
CREATE DATABASE gameserverdb;
```

## User_Account TABLE

```mssql
IF OBJECT_ID('user_account', 'U') IS NOT NULL DROP TABLE user_account;
CREATE TABLE user_account (
    user_index INT IDENTITY(1,1) NOT NULL PRIMARY KEY,                     -- ���� ���� ID
    member_id NVARCHAR(20) NULL UNIQUE,                      -- ī��24 ��� ���̵� (�Խ�Ʈ�� NULL)
    device_id NVARCHAR(100) NOT NULL,                        -- ����̽� ID
    nickname NVARCHAR(15) NOT NULL,                          -- �г���
    is_banned BIT NOT NULL DEFAULT 0,                        -- ���� ���� ����: FALSE=����, TRUE=������
    is_delete BIT NOT NULL DEFAULT 0,                        -- ���� ���� ����: FALSE=����, TRUE=������
    created_at DATETIME NOT NULL DEFAULT GETDATE(),          -- ���� ���� �ð� (KST)
    last_login_at DATETIME NOT NULL DEFAULT GETDATE()        -- ������ �α��� �ð� (KST)
);
CREATE INDEX idx_device_id ON user_account (device_id);

EXEC sp_addextendedproperty N'MS_Description', N'���� ���� ���� ���̺�', 'SCHEMA', N'dbo', 'TABLE', N'user_account';
EXEC sp_addextendedproperty N'MS_Description', N'���� ���� ID', 'SCHEMA', N'dbo', 'TABLE', N'user_account', 'COLUMN', N'user_index';
EXEC sp_addextendedproperty N'MS_Description', N'ī��24 ��� ���̵� (�Խ�Ʈ�� NULL)', 'SCHEMA', N'dbo', 'TABLE', N'user_account', 'COLUMN', N'member_id';
EXEC sp_addextendedproperty N'MS_Description', N'����̽� ID', 'SCHEMA', N'dbo', 'TABLE', N'user_account', 'COLUMN', N'device_id';
EXEC sp_addextendedproperty N'MS_Description', N'�г���', 'SCHEMA', N'dbo', 'TABLE', N'user_account', 'COLUMN', N'nickname';
EXEC sp_addextendedproperty N'MS_Description', N'���� ���� ����: FALSE=����, TRUE=������', 'SCHEMA', N'dbo', 'TABLE', N'user_account', 'COLUMN', N'is_banned';
EXEC sp_addextendedproperty N'MS_Description', N'���� ���� ����: FALSE=����, TRUE=������', 'SCHEMA', N'dbo', 'TABLE', N'user_account', 'COLUMN', N'is_delete';
EXEC sp_addextendedproperty N'MS_Description', N'���� ���� �ð� (KST)', 'SCHEMA', N'dbo', 'TABLE', N'user_account', 'COLUMN', N'created_at';
EXEC sp_addextendedproperty N'MS_Description', N'������ �α��� �ð� (KST)', 'SCHEMA', N'dbo', 'TABLE', N'user_account', 'COLUMN', N'last_login_at';
```

## User_Character Table

``` mssql
IF OBJECT_ID('user_character', 'U') IS NOT NULL DROP TABLE user_character;
CREATE TABLE user_character (
    user_index INT NOT NULL,                                 -- ����� ���� �ĺ���
    character_index INT NOT NULL,                            -- ĳ���� ���� �ĺ���
    level INT NOT NULL DEFAULT 0,                            -- ĳ���� ����
    exp INT NOT NULL DEFAULT 0,                              -- ĳ���� ����ġ
    is_active BIT NOT NULL DEFAULT 0,                        -- ���� Ű��� ������ ����
    is_acquired BIT NOT NULL DEFAULT 0,                      -- ĳ���͸� ȹ���Ͽ����� ����
    play_days_ INT NOT NULL DEFAULT 0,                       -- ĳ���� �Բ��� �ϼ�
    equipped_at DATETIME NOT NULL DEFAULT GETDATE(),         -- ĳ���� ���� ��¥
    acquired_at DATETIME NULL,                               -- ĳ���� ȹ�� ��¥
    CONSTRAINT PK_user_character PRIMARY KEY (user_index, character_index)
);

EXEC sp_addextendedproperty N'MS_Description', N'���� ĳ���� ���� ���̺�', 'SCHEMA', N'dbo', 'TABLE', N'user_character';
EXEC sp_addextendedproperty N'MS_Description', N'����� ���� �ĺ���', 'SCHEMA', N'dbo', 'TABLE', N'user_character', 'COLUMN', N'user_index';
EXEC sp_addextendedproperty N'MS_Description', N'ĳ���� ���� �ĺ���', 'SCHEMA', N'dbo', 'TABLE', N'user_character', 'COLUMN', N'character_index';
EXEC sp_addextendedproperty N'MS_Description', N'ĳ���� ����', 'SCHEMA', N'dbo', 'TABLE', N'user_character', 'COLUMN', N'level';
EXEC sp_addextendedproperty N'MS_Description', N'ĳ���� ����ġ', 'SCHEMA', N'dbo', 'TABLE', N'user_character', 'COLUMN', N'exp';
EXEC sp_addextendedproperty N'MS_Description', N'���� Ű��� ������ ����', 'SCHEMA', N'dbo', 'TABLE', N'user_character', 'COLUMN', N'is_active';
EXEC sp_addextendedproperty N'MS_Description', N'ĳ���͸� ȹ���Ͽ����� ����', 'SCHEMA', N'dbo', 'TABLE', N'user_character', 'COLUMN', N'is_acquired';
EXEC sp_addextendedproperty N'MS_Description', N'ĳ���� �Բ��� �ϼ�', 'SCHEMA', N'dbo', 'TABLE', N'user_character', 'COLUMN', N'play_days_';
EXEC sp_addextendedproperty N'MS_Description', N'ĳ���� ���� ��¥', 'SCHEMA', N'dbo', 'TABLE', N'user_character', 'COLUMN', N'equipped_at';
EXEC sp_addextendedproperty N'MS_Description', N'ĳ���� ȹ�� ��¥', 'SCHEMA', N'dbo', 'TABLE', N'user_character', 'COLUMN', N'acquired_at';
```

## User_Daily_Mission Table

```mssql
IF OBJECT_ID('user_daily_mission', 'U') IS NOT NULL DROP TABLE user_daily_mission;
CREATE TABLE user_daily_mission (
    user_index INT NOT NULL, -- ����� ���� �ĺ���
    daily_mission_index INT NOT NULL, -- ���� �̼� ���� �ĺ���
    goods_type INT NOT NULL, -- ��ȭ Ÿ��
    goods_index INT NOT NULL, -- ��ȭ ���� �ĺ���
    mission_goal_count TINYINT NOT NULL DEFAULT 0, -- ���� �Ϸ� Ƚ��
    mission_progress TINYINT NOT NULL DEFAULT 0, -- ���� ���� Ƚ��
    is_received BIT NOT NULL DEFAULT 0, -- ���� ���� ����
    updated_at DATETIME NOT NULL DEFAULT GETDATE(), -- ���� �Ϸ� ��¥ - ���� ������ �ʱ�ȭ
    CONSTRAINT PK_user_daily_mission PRIMARY KEY (user_index, daily_mission_index)
);

EXEC sp_addextendedproperty N'MS_Description', N'���� ���� �̼� ���̺�', 'SCHEMA', N'dbo', 'TABLE', N'user_daily_mission';
EXEC sp_addextendedproperty N'MS_Description', N'����� ���� �ĺ���', 'SCHEMA', N'dbo', 'TABLE', N'user_daily_mission', 'COLUMN', N'user_index';
EXEC sp_addextendedproperty N'MS_Description', N'���� �̼� ���� �ĺ���', 'SCHEMA', N'dbo', 'TABLE', N'user_daily_mission', 'COLUMN', N'daily_mission_index';
EXEC sp_addextendedproperty N'MS_Description', N'��ȭ Ÿ��', 'SCHEMA', N'dbo', 'TABLE', N'user_daily_mission', 'COLUMN', N'goods_type';
EXEC sp_addextendedproperty N'MS_Description', N'��ȭ ���� �ĺ���', 'SCHEMA', N'dbo', 'TABLE', N'user_daily_mission', 'COLUMN', N'goods_index';
EXEC sp_addextendedproperty N'MS_Description', N'���� �Ϸ� Ƚ��', 'SCHEMA', N'dbo', 'TABLE', N'user_daily_mission', 'COLUMN', N'mission_goal_count';
EXEC sp_addextendedproperty N'MS_Description', N'���� ���� Ƚ��', 'SCHEMA', N'dbo', 'TABLE', N'user_daily_mission', 'COLUMN', N'mission_progress';
EXEC sp_addextendedproperty N'MS_Description', N'���� ���� ����', 'SCHEMA', N'dbo', 'TABLE', N'user_daily_mission', 'COLUMN', N'is_received';
EXEC sp_addextendedproperty N'MS_Description', N'���� �Ϸ� ��¥ - ���� ������ �ʱ�ȭ', 'SCHEMA', N'dbo', 'TABLE', N'user_daily_mission', 'COLUMN', N'updated_at';
```

## User_Equip Table

```mssql
IF OBJECT_ID('user_equip', 'U') IS NOT NULL DROP TABLE user_equip;
CREATE TABLE user_equip (
    user_index INT NOT NULL, -- ����� ���� �ĺ���
    character_index INT NOT NULL, -- ĳ���� ID
    item_type TINYINT NOT NULL, -- ������ Type
    item_index INT NOT NULL, -- ������ ID
    CONSTRAINT PK_user_equip PRIMARY KEY (user_index, character_index, item_index)
);

EXEC sp_addextendedproperty N'MS_Description', N'���� ĳ���� ���� ���̺�', 'SCHEMA', N'dbo', 'TABLE', N'user_equip';
EXEC sp_addextendedproperty N'MS_Description', N'����� ���� �ĺ���', 'SCHEMA', N'dbo', 'TABLE', N'user_equip', 'COLUMN', N'user_index';
EXEC sp_addextendedproperty N'MS_Description', N'ĳ���� ID', 'SCHEMA', N'dbo', 'TABLE', N'user_equip', 'COLUMN', N'character_index';
EXEC sp_addextendedproperty N'MS_Description', N'������ Type', 'SCHEMA', N'dbo', 'TABLE', N'user_equip', 'COLUMN', N'item_type';
EXEC sp_addextendedproperty N'MS_Description', N'������ ID', 'SCHEMA', N'dbo', 'TABLE', N'user_equip', 'COLUMN', N'item_index';
```

## User_Goods Table

``` mssql
IF OBJECT_ID('user_goods', 'U') IS NOT NULL DROP TABLE user_goods;
CREATE TABLE user_goods (
    user_index INT NOT NULL, -- ����� ���� �ĺ���
    goods_index TINYINT NOT NULL, -- ��ȭ ���� �ĺ���
    quantity INT NOT NULL DEFAULT 0, -- ���� ����
    CONSTRAINT PK_user_goods PRIMARY KEY (user_index, goods_index)
);

EXEC sp_addextendedproperty N'MS_Description', N'���� ��ȭ ���̺�', 'SCHEMA', N'dbo', 'TABLE', N'user_goods';
EXEC sp_addextendedproperty N'MS_Description', N'����� ���� �ĺ���', 'SCHEMA', N'dbo', 'TABLE', N'user_goods', 'COLUMN', N'user_index';
EXEC sp_addextendedproperty N'MS_Description', N'��ȭ ���� �ĺ���', 'SCHEMA', N'dbo', 'TABLE', N'user_goods', 'COLUMN', N'goods_index';
EXEC sp_addextendedproperty N'MS_Description', N'���� ����', 'SCHEMA', N'dbo', 'TABLE', N'user_goods', 'COLUMN', N'quantity';
```

## User_Invetory Table

```mssql
IF OBJECT_ID('user_inventory', 'U') IS NOT NULL DROP TABLE user_inventory;
CREATE TABLE user_inventory (
    user_index INT NOT NULL, -- ����� ���� �ĺ���
    item_index INT NOT NULL, -- ������ ���� �ĺ���
    owned_at DATETIME NOT NULL DEFAULT GETDATE(), -- ������ ȹ�� ��¥
    CONSTRAINT PK_user_inventory PRIMARY KEY (user_index, item_index)
);

EXEC sp_addextendedproperty N'MS_Description', N'���� ���� ������ ���̺�', 'SCHEMA', N'dbo', 'TABLE', N'user_inventory';
EXEC sp_addextendedproperty N'MS_Description', N'����� ���� �ĺ���', 'SCHEMA', N'dbo', 'TABLE', N'user_inventory', 'COLUMN', N'user_index';
EXEC sp_addextendedproperty N'MS_Description', N'������ ���� �ĺ���', 'SCHEMA', N'dbo', 'TABLE', N'user_inventory', 'COLUMN', N'item_index';
EXEC sp_addextendedproperty N'MS_Description', N'������ ȹ�� ��¥', 'SCHEMA', N'dbo', 'TABLE', N'user_inventory', 'COLUMN', N'owned_at';
```

## User_Summon_State Table

```mssql

���� �ε���
Avail �̱� Ƚ��
�� ���� Ƚ��
```