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
    user_index INT IDENTITY(1,1) NOT NULL PRIMARY KEY,                     -- 유저 고유 ID
    member_id NVARCHAR(20) NULL UNIQUE,                      -- 카페24 멤버 아이디 (게스트는 NULL)
    device_id NVARCHAR(100) NOT NULL,                        -- 디바이스 ID
    nickname NVARCHAR(15) NOT NULL,                          -- 닉네임
    is_banned BIT NOT NULL DEFAULT 0,                        -- 계정 정지 여부: FALSE=정상, TRUE=정지됨
    is_delete BIT NOT NULL DEFAULT 0,                        -- 계정 삭제 여부: FALSE=정상, TRUE=삭제됨
    created_at DATETIME NOT NULL DEFAULT GETDATE(),          -- 계정 생성 시각 (KST)
    last_login_at DATETIME NOT NULL DEFAULT GETDATE()        -- 마지막 로그인 시각 (KST)
);
CREATE INDEX idx_device_id ON user_account (device_id);

EXEC sp_addextendedproperty N'MS_Description', N'유저 계정 정보 테이블', 'SCHEMA', N'dbo', 'TABLE', N'user_account';
EXEC sp_addextendedproperty N'MS_Description', N'유저 고유 ID', 'SCHEMA', N'dbo', 'TABLE', N'user_account', 'COLUMN', N'user_index';
EXEC sp_addextendedproperty N'MS_Description', N'카페24 멤버 아이디 (게스트는 NULL)', 'SCHEMA', N'dbo', 'TABLE', N'user_account', 'COLUMN', N'member_id';
EXEC sp_addextendedproperty N'MS_Description', N'디바이스 ID', 'SCHEMA', N'dbo', 'TABLE', N'user_account', 'COLUMN', N'device_id';
EXEC sp_addextendedproperty N'MS_Description', N'닉네임', 'SCHEMA', N'dbo', 'TABLE', N'user_account', 'COLUMN', N'nickname';
EXEC sp_addextendedproperty N'MS_Description', N'계정 정지 여부: FALSE=정상, TRUE=정지됨', 'SCHEMA', N'dbo', 'TABLE', N'user_account', 'COLUMN', N'is_banned';
EXEC sp_addextendedproperty N'MS_Description', N'계정 삭제 여부: FALSE=정상, TRUE=삭제됨', 'SCHEMA', N'dbo', 'TABLE', N'user_account', 'COLUMN', N'is_delete';
EXEC sp_addextendedproperty N'MS_Description', N'계정 생성 시각 (KST)', 'SCHEMA', N'dbo', 'TABLE', N'user_account', 'COLUMN', N'created_at';
EXEC sp_addextendedproperty N'MS_Description', N'마지막 로그인 시각 (KST)', 'SCHEMA', N'dbo', 'TABLE', N'user_account', 'COLUMN', N'last_login_at';
```

## User_Character Table

``` mssql
IF OBJECT_ID('user_character', 'U') IS NOT NULL DROP TABLE user_character;
CREATE TABLE user_character (
    user_index INT NOT NULL,                                 -- 사용자 고유 식별자
    character_index INT NOT NULL,                            -- 캐릭터 고유 식별자
    level INT NOT NULL DEFAULT 0,                            -- 캐릭터 레벨
    exp INT NOT NULL DEFAULT 0,                              -- 캐릭터 경험치
    is_active BIT NOT NULL DEFAULT 0,                        -- 현재 키우는 중인지 여부
    is_acquired BIT NOT NULL DEFAULT 0,                      -- 캐릭터를 획득하였는지 여부
    play_days_ INT NOT NULL DEFAULT 0,                       -- 캐릭터 함께한 일수
    equipped_at DATETIME NOT NULL DEFAULT GETDATE(),         -- 캐릭터 장착 날짜
    acquired_at DATETIME NULL,                               -- 캐릭터 획득 날짜
    CONSTRAINT PK_user_character PRIMARY KEY (user_index, character_index)
);

EXEC sp_addextendedproperty N'MS_Description', N'유저 캐릭터 정보 테이블', 'SCHEMA', N'dbo', 'TABLE', N'user_character';
EXEC sp_addextendedproperty N'MS_Description', N'사용자 고유 식별자', 'SCHEMA', N'dbo', 'TABLE', N'user_character', 'COLUMN', N'user_index';
EXEC sp_addextendedproperty N'MS_Description', N'캐릭터 고유 식별자', 'SCHEMA', N'dbo', 'TABLE', N'user_character', 'COLUMN', N'character_index';
EXEC sp_addextendedproperty N'MS_Description', N'캐릭터 레벨', 'SCHEMA', N'dbo', 'TABLE', N'user_character', 'COLUMN', N'level';
EXEC sp_addextendedproperty N'MS_Description', N'캐릭터 경험치', 'SCHEMA', N'dbo', 'TABLE', N'user_character', 'COLUMN', N'exp';
EXEC sp_addextendedproperty N'MS_Description', N'현재 키우는 중인지 여부', 'SCHEMA', N'dbo', 'TABLE', N'user_character', 'COLUMN', N'is_active';
EXEC sp_addextendedproperty N'MS_Description', N'캐릭터를 획득하였는지 여부', 'SCHEMA', N'dbo', 'TABLE', N'user_character', 'COLUMN', N'is_acquired';
EXEC sp_addextendedproperty N'MS_Description', N'캐릭터 함께한 일수', 'SCHEMA', N'dbo', 'TABLE', N'user_character', 'COLUMN', N'play_days_';
EXEC sp_addextendedproperty N'MS_Description', N'캐릭터 장착 날짜', 'SCHEMA', N'dbo', 'TABLE', N'user_character', 'COLUMN', N'equipped_at';
EXEC sp_addextendedproperty N'MS_Description', N'캐릭터 획득 날짜', 'SCHEMA', N'dbo', 'TABLE', N'user_character', 'COLUMN', N'acquired_at';
```

## User_Daily_Mission Table

```mssql
IF OBJECT_ID('user_daily_mission', 'U') IS NOT NULL DROP TABLE user_daily_mission;
CREATE TABLE user_daily_mission (
    user_index INT NOT NULL, -- 사용자 고유 식별자
    daily_mission_index INT NOT NULL, -- 일일 미션 고유 식별자
    goods_type INT NOT NULL, -- 재화 타입
    goods_index INT NOT NULL, -- 재화 고유 식별자
    mission_goal_count TINYINT NOT NULL DEFAULT 0, -- 수행 완료 횟수
    mission_progress TINYINT NOT NULL DEFAULT 0, -- 수행 진행 횟수
    is_received BIT NOT NULL DEFAULT 0, -- 보상 수령 여부
    updated_at DATETIME NOT NULL DEFAULT GETDATE(), -- 수령 완료 날짜 - 날이 지나면 초기화
    CONSTRAINT PK_user_daily_mission PRIMARY KEY (user_index, daily_mission_index)
);

EXEC sp_addextendedproperty N'MS_Description', N'유저 일일 미션 테이블', 'SCHEMA', N'dbo', 'TABLE', N'user_daily_mission';
EXEC sp_addextendedproperty N'MS_Description', N'사용자 고유 식별자', 'SCHEMA', N'dbo', 'TABLE', N'user_daily_mission', 'COLUMN', N'user_index';
EXEC sp_addextendedproperty N'MS_Description', N'일일 미션 고유 식별자', 'SCHEMA', N'dbo', 'TABLE', N'user_daily_mission', 'COLUMN', N'daily_mission_index';
EXEC sp_addextendedproperty N'MS_Description', N'재화 타입', 'SCHEMA', N'dbo', 'TABLE', N'user_daily_mission', 'COLUMN', N'goods_type';
EXEC sp_addextendedproperty N'MS_Description', N'재화 고유 식별자', 'SCHEMA', N'dbo', 'TABLE', N'user_daily_mission', 'COLUMN', N'goods_index';
EXEC sp_addextendedproperty N'MS_Description', N'수행 완료 횟수', 'SCHEMA', N'dbo', 'TABLE', N'user_daily_mission', 'COLUMN', N'mission_goal_count';
EXEC sp_addextendedproperty N'MS_Description', N'수행 진행 횟수', 'SCHEMA', N'dbo', 'TABLE', N'user_daily_mission', 'COLUMN', N'mission_progress';
EXEC sp_addextendedproperty N'MS_Description', N'보상 수령 여부', 'SCHEMA', N'dbo', 'TABLE', N'user_daily_mission', 'COLUMN', N'is_received';
EXEC sp_addextendedproperty N'MS_Description', N'수령 완료 날짜 - 날이 지나면 초기화', 'SCHEMA', N'dbo', 'TABLE', N'user_daily_mission', 'COLUMN', N'updated_at';
```

## User_Equip Table

```mssql
IF OBJECT_ID('user_equip', 'U') IS NOT NULL DROP TABLE user_equip;
CREATE TABLE user_equip (
    user_index INT NOT NULL, -- 사용자 고유 식별자
    character_index INT NOT NULL, -- 캐릭터 ID
    item_type TINYINT NOT NULL, -- 아이템 Type
    item_index INT NOT NULL, -- 아이템 ID
    CONSTRAINT PK_user_equip PRIMARY KEY (user_index, character_index, item_index)
);

EXEC sp_addextendedproperty N'MS_Description', N'유저 캐릭터 장착 테이블', 'SCHEMA', N'dbo', 'TABLE', N'user_equip';
EXEC sp_addextendedproperty N'MS_Description', N'사용자 고유 식별자', 'SCHEMA', N'dbo', 'TABLE', N'user_equip', 'COLUMN', N'user_index';
EXEC sp_addextendedproperty N'MS_Description', N'캐릭터 ID', 'SCHEMA', N'dbo', 'TABLE', N'user_equip', 'COLUMN', N'character_index';
EXEC sp_addextendedproperty N'MS_Description', N'아이템 Type', 'SCHEMA', N'dbo', 'TABLE', N'user_equip', 'COLUMN', N'item_type';
EXEC sp_addextendedproperty N'MS_Description', N'아이템 ID', 'SCHEMA', N'dbo', 'TABLE', N'user_equip', 'COLUMN', N'item_index';
```

## User_Goods Table

``` mssql
IF OBJECT_ID('user_goods', 'U') IS NOT NULL DROP TABLE user_goods;
CREATE TABLE user_goods (
    user_index INT NOT NULL, -- 사용자 고유 식별자
    goods_index TINYINT NOT NULL, -- 재화 고유 식별자
    quantity INT NOT NULL DEFAULT 0, -- 보유 개수
    CONSTRAINT PK_user_goods PRIMARY KEY (user_index, goods_index)
);

EXEC sp_addextendedproperty N'MS_Description', N'유저 재화 테이블', 'SCHEMA', N'dbo', 'TABLE', N'user_goods';
EXEC sp_addextendedproperty N'MS_Description', N'사용자 고유 식별자', 'SCHEMA', N'dbo', 'TABLE', N'user_goods', 'COLUMN', N'user_index';
EXEC sp_addextendedproperty N'MS_Description', N'재화 고유 식별자', 'SCHEMA', N'dbo', 'TABLE', N'user_goods', 'COLUMN', N'goods_index';
EXEC sp_addextendedproperty N'MS_Description', N'보유 개수', 'SCHEMA', N'dbo', 'TABLE', N'user_goods', 'COLUMN', N'quantity';
```

## User_Invetory Table

```mssql
IF OBJECT_ID('user_inventory', 'U') IS NOT NULL DROP TABLE user_inventory;
CREATE TABLE user_inventory (
    user_index INT NOT NULL, -- 사용자 고유 식별자
    item_index INT NOT NULL, -- 아이템 고유 식별자
    owned_at DATETIME NOT NULL DEFAULT GETDATE(), -- 아이템 획득 날짜
    CONSTRAINT PK_user_inventory PRIMARY KEY (user_index, item_index)
);

EXEC sp_addextendedproperty N'MS_Description', N'유저 보유 아이템 테이블', 'SCHEMA', N'dbo', 'TABLE', N'user_inventory';
EXEC sp_addextendedproperty N'MS_Description', N'사용자 고유 식별자', 'SCHEMA', N'dbo', 'TABLE', N'user_inventory', 'COLUMN', N'user_index';
EXEC sp_addextendedproperty N'MS_Description', N'아이템 고유 식별자', 'SCHEMA', N'dbo', 'TABLE', N'user_inventory', 'COLUMN', N'item_index';
EXEC sp_addextendedproperty N'MS_Description', N'아이템 획득 날짜', 'SCHEMA', N'dbo', 'TABLE', N'user_inventory', 'COLUMN', N'owned_at';
```

## User_Summon_State Table

```mssql

유저 인덱스
Avail 뽑기 횟수
총 뽑은 횟수
```