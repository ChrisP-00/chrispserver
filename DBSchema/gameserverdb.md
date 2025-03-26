# [ GameServer DB ]
  
## Database Creation

```sql
DROP DATABASE IF EXISTS gameserverdb;
CREATE DATABASE IF NOT EXISTS gameserverdb;
```

## User_Account TABLE

```sql
DROP TABLE IF EXISTS gameserverdb.`user_account`;
CREATE TABLE IF NOT EXISTS gameserverdb.`user_account` (
    user_index                  INT          NOT NULL AUTO_INCREMENT PRIMARY KEY COMMENT '���� ���� ID',
    member_id_                  VARCHAR(20)  NOT NULL 
    unity_device_number         VARCHAR(100) NOT NULL COMMENT '����Ƽ ����̽� ID',
    nickname                    VARCHAR(15)  NOT NULL COMMENT '�г���',
    is_banned                   BOOLEAN      NOT NULL DEFAULT FALSE COMMENT '���� ���� ����: FALSE=����, TRUE=������',
    is_delete                   BOOLEAN      NOT NULL DEFAULT FALSE COMMENT '���� ���� ����: FALSE=����, TRUE=������',
    created_at                  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '���� ���� �ð� (KST)',
    last_login_at               DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '������ �α��� �ð� (KST)'
) COMMENT='���� ���� ���� ���̺�';
```

## User_Character Table

``` sql
DROP TABLE IF EXISTS gameserverdb.`user_character`;
CREATE TABLE IF NOT EXISTS gameserverdb.`user_character` (
    user_index	                INT	         NOT NULL PRIMARY KEY   COMMENT '����� ���� �ĺ���',
    character_index	            INT	         NOT NULL UNIQUE KEY    COMMENT 'ĳ���� ���� �ĺ���',
    level	                    INT	         NOT NULL DEFAULT TRUE  COMMENT 'ĳ���� ����',
    exp	                        INT	         NOT NULL DEFAULT FALSE COMMENT 'ĳ���� ����ġ',
    dirt_level	                INT	         NOT NULL DEFAULT FALSE COMMENT '������ ����',
    quantity	                INT	         NOT NULL DEFAULT TRUE  COMMENT 'ĳ���� ���� ����',
    is_active	                BOOL         NOT NULL DEFAULT FALSE COMMENT	'���� Ű��� ������ ���� (0 = �ƴ�, 1 = Ű��� ��)',
    owned_at	                DATETIME DEFAULT CURRENT_TIMESTAMP	COMMENT	'ĳ���� ���� ��¥	'
) COMMENT='���� ĳ���� ���� ���̺�';
```

## User_Goods Table

``` sql
DROP TABLE IF EXISTS gameserverdb.`user_goods`;
CREATE TABLE IF NOT EXISTS gameserverdb.`user_goods` (
    user_index	                INT	        NOT NULL PRIMARY KEY    COMMENT '����� ���� �ĺ���',
    goods_index	                TINYINT     NOT NULL UNIQUE KEY     COMMENT '��ȭ ���� �ĺ���',
    quantity	                INT		    NOT NULL                COMMENT	'���� ����'  
) COMMENT='���� ��ȭ ���̺�';
```


## User_Invetory Table

```sql
DROP TABLE IF EXISTS gameserverdb.`user_inventory`;
CREATE TABLE IF NOT EXISTS gameserverdb.`user_inventory` (
    user_index                  INT         NOT NULL PRIMARY KEY               COMMENT '����� ���� �ĺ���',
    item_index                  INT         NOT NULL UNIQUE KEY                COMMENT '������ ���� �ĺ���',
    owned_at                    DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '������ ȹ�� ��¥'
) COMMENT='���� ���� ������ ���̺�';
```


## User_Equip Table

```sql
DROP TABLE IF EXISTS gameserverdb.`user_equip`;
CREATE TABLE IF NOT EXISTS gameserverdb.`user_equip` (
    user_index        INT      NOT NULL PRIMARY KEY COMMENT '����� ���� �ĺ���',
    character_index   INT      NOT NULL             COMMENT 'ĳ���� ID',
    item_index        INT      NOT NULL             COMMENT '������ ID',
    is_equipped       BOOL     NOT NULL             DEFAULT FALSE COMMENT '���� ���� (0 = ������, 1 = ����)'
) COMMENT='���� ĳ���� ���� ���̺�';
```


## User_Daily_Mission Table

```sql
DROP TABLE IF EXISTS gameserverdb.`user_daily_mission`;
CREATE TABLE IF NOT EXISTS gameserverdb.`user_daily_mission` (
    user_index            INT        NOT NULL COMMENT '����� ���� �ĺ���',
    daily_mission_index   INT        NOT NULL COMMENT '���� �̼� ���� �ε���',
    amount                TINYINT    NOT NULL DEFAULT 0 COMMENT '���� Ƚ��',
    is_received          BOOL       NOT NULL DEFAULT FALSE COMMENT '���� ����',
    received_at          DATETIME   COMMENT '���� �Ϸ� ��¥ - ���� ������ �ʱ�ȭ',
    PRIMARY KEY (user_index)
) COMMENT='���� ���� �̼� ���̺�';
```

## User_Character_Mission Table

```sql
DROP TABLE IF EXISTS gameserverdb.`user_character_mission`;
CREATE TABLE IF NOT EXISTS gameserverdb.`user_character_mission` (
    user_index                 INT        NOT NULL COMMENT '����� ���� �ĺ���',
    character_mission_index   INT        NOT NULL COMMENT 'ĳ���� �̼� ���� �ε���',
    amount                    INT        NOT NULL COMMENT '���� Ƚ��',
    is_received              BOOL       NOT NULL DEFAULT FALSE COMMENT '���� ����',
    received_at              DATETIME   COMMENT '���� �Ϸ� ��¥',
    PRIMARY KEY (user_index)
) COMMENT='ĳ���� �̼� ���̺�';
```