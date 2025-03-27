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
    member_id                   VARCHAR(20)  NOT NULL   COMMENT 'ī��24 ��� ���̵�',
    unity_device_number         VARCHAR(100) NOT NULL   COMMENT '����Ƽ ����̽� ID',
    nickname                    VARCHAR(15)  NOT NULL   COMMENT '�г���',
    is_banned                   BOOLEAN      NOT NULL DEFAULT FALSE     COMMENT '���� ���� ����: FALSE=����, TRUE=������',
    is_delete                   BOOLEAN      NOT NULL DEFAULT FALSE     COMMENT '���� ���� ����: FALSE=����, TRUE=������',
    created_at                  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP     COMMENT '���� ���� �ð� (KST)',
    last_login_at               DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP     COMMENT '������ �α��� �ð� (KST)'
) COMMENT='���� ���� ���� ���̺�';
```

## User_Character Table

``` sql
DROP TABLE IF EXISTS gameserverdb.`user_character`;
CREATE TABLE IF NOT EXISTS gameserverdb.`user_character` (
    user_index	                INT	         NOT NULL   COMMENT '����� ���� �ĺ���',
    character_index	            INT	         NOT NULL   COMMENT 'ĳ���� ���� �ĺ���',
    level	                    INT	         NOT NULL DEFAULT 0     COMMENT 'ĳ���� ����',
    exp	                        INT	         NOT NULL DEFAULT 0     COMMENT 'ĳ���� ����ġ',
    quantity	                INT	         NOT NULL DEFAULT 1     COMMENT 'ĳ���� ���� ����',
    is_active	                BOOL         NOT NULL DEFAULT FALSE     COMMENT	'���� Ű��� ������ ���� (0 = �ƴ�, 1 = Ű��� ��)',
    owned_at	                DATETIME DEFAULT CURRENT_TIMESTAMP	    COMMENT	'ĳ���� ���� ��¥	',
    PRIMARY KEY (user_index, character_index)
) COMMENT='���� ĳ���� ���� ���̺�';
```

## User_Goods Table

``` sql
DROP TABLE IF EXISTS gameserverdb.`user_goods`;
CREATE TABLE IF NOT EXISTS gameserverdb.`user_goods` (
    user_index	                INT	        NOT NULL    COMMENT '����� ���� �ĺ���',
    goods_index	                TINYINT     NOT NULL    COMMENT '��ȭ ���� �ĺ���',
    quantity	                INT		    NOT NULL DEFAULT 0  COMMENT	'���� ����',
    PRIMARY KEY (user_index, goods_index)
) COMMENT='���� ��ȭ ���̺�';
```


## User_Invetory Table

```sql
DROP TABLE IF EXISTS gameserverdb.`user_inventory`;
CREATE TABLE IF NOT EXISTS gameserverdb.`user_inventory` (
    user_index                  INT         NOT NULL    COMMENT '����� ���� �ĺ���',
    item_index                  INT         NOT NULL    COMMENT '������ ���� �ĺ���',
    owned_at                    DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP    COMMENT '������ ȹ�� ��¥',
    PRIMARY KEY (user_index, item_index)
) COMMENT='���� ���� ������ ���̺�';
```


## User_Equip Table

```sql
DROP TABLE IF EXISTS gameserverdb.`user_equip`;
CREATE TABLE IF NOT EXISTS gameserverdb.`user_equip` (
    user_index        INT      NOT NULL     COMMENT '����� ���� �ĺ���',
    character_index   INT      NOT NULL     COMMENT 'ĳ���� ID',
    item_index        INT      NOT NULL     COMMENT '������ ID',
    is_equipped       BOOL     NOT NULL DEFAULT FALSE      COMMENT '���� ���� (0 = ������, 1 = ����)',
    PRIMARY KEY (user_index, character_index, item_index)
) COMMENT='���� ĳ���� ���� ���̺�';
```


## User_Daily_Mission Table

```sql
DROP TABLE IF EXISTS gameserverdb.`user_daily_mission`;
CREATE TABLE IF NOT EXISTS gameserverdb.`user_daily_mission` (
    user_index          INT NOT NULL    COMMENT '����� ���� �ĺ���',
    mission_index       INT NOT NULL    COMMENT '���� �̼� ���� �ε���',
    amount              INT NOT NULL DEFAULT 0  COMMENT '���� Ƚ��',
    is_received         BOOL NOT NULL DEFAULT FALSE COMMENT '���� ����',
    received_at         DATETIME    COMMENT '���� �Ϸ� ��¥ - ���� ������ �ʱ�ȭ',
    PRIMARY KEY (user_index, mission_index)
) COMMENT='���� ���� �̼� ���̺�';
```

## User_Character_Mission Table

```sql
DROP TABLE IF EXISTS gameserverdb.`user_character_mission`;
CREATE TABLE IF NOT EXISTS gameserverdb.`user_character_mission` (
    user_index          INT NOT NULL    COMMENT '����� ���� �ĺ���',
    mission_index       INT NOT NULL    COMMENT 'ĳ���� �̼� ���� �ε���',
    quantity            INT NOT NULL    COMMENT '���� Ƚ��',
    is_received         BOO NOT NULL DEFAULT FALSE  COMMENT '���� ����',
    received_at         DATETIME   COMMENT '���� �Ϸ� ��¥',
    PRIMARY KEY (user_index, mission_index)
) COMMENT='ĳ���� �̼� ���̺�';
```

## User_Summon_State Table

```sql

���� �ε���
Avail �̱� Ƚ��
�� ���� Ƚ��
```