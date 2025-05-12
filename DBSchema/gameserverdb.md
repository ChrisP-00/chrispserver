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
    user_index                  INT          NOT NULL AUTO_INCREMENT PRIMARY KEY     COMMENT '���� ���� ID',
    member_id                   VARCHAR(20)  NULL     UNIQUE                         COMMENT 'ī��24 ��� ���̵� (�Խ�Ʈ�� NULL)',
    device_id                   VARCHAR(100) NOT NULL   COMMENT '����̽� ID',
    nickname                    VARCHAR(15)  NOT NULL   COMMENT '�г���',
    is_banned                   BOOLEAN      NOT NULL DEFAULT FALSE     COMMENT '���� ���� ����: FALSE=����, TRUE=������',
    is_delete                   BOOLEAN      NOT NULL DEFAULT FALSE     COMMENT '���� ���� ����: FALSE=����, TRUE=������',
    created_at                  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP     COMMENT '���� ���� �ð� (KST)',
    last_login_at               DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP     COMMENT '������ �α��� �ð� (KST)'

    INDEX idx_device_id (device_id)
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
    is_active	                BOOL         NOT NULL DEFAULT FALSE     COMMENT	'���� Ű��� ������ ����',
    is_acquired                 BOOL         NOT NULL DEFAULT FALSE     COMMENT 'ĳ���͸� ȹ���Ͽ����� ����',
    play_days_  	            INT	         NOT NULL DEFAULT 0     COMMENT 'ĳ���� �Բ��� �ϼ�',
    equipped_at                 DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'ĳ���� ���� ��¥',
    acquired_at	                DATETIME     DEFAULT NULL	COMMENT	'ĳ���� ȹ�� ��¥	',
    PRIMARY KEY (user_index, character_index)
) COMMENT='���� ĳ���� ���� ���̺�';
```

## User_Daily_Mission Table

```sql
DROP TABLE IF EXISTS gameserverdb.`user_daily_mission`;
CREATE TABLE IF NOT EXISTS gameserverdb.`user_daily_mission` (
    user_index              INT     NOT NULL    COMMENT '����� ���� �ĺ���',
    daily_mission_index     INT     NOT NULL    COMMENT '���� �̼� ���� �ĺ���',
    goods_type              INT     NOT NULL    COMMENT '��ȭ Ÿ��',
    goods_index             INT     NOT NULL    COMMENT '��ȭ ���� �ĺ���',
    mission_gaol_count      TINYINT NOT NULL    DEFAULT 0   COMMENT '���� �Ϸ� Ƚ��',
    mission_progress        TINYINT NOT NULL    DEFAULT 0   COMMENT '���� ���� Ƚ��',
    is_received             BOOL    NOT NULL    DEFAULT FALSE   COMMENT '���� ���� ����',
    updated_at              DATETIME DEFAULT CURRENT_TIMESTAMP  COMMENT '���� �Ϸ� ��¥ - ���� ������ �ʱ�ȭ',
    PRIMARY KEY (user_index, mission_index)
) COMMENT='���� ���� �̼� ���̺�';
```

## User_Equip Table

```sql
DROP TABLE IF EXISTS gameserverdb.`user_equip`;
CREATE TABLE IF NOT EXISTS gameserverdb.`user_equip` (
    user_index        INT      NOT NULL     COMMENT '����� ���� �ĺ���',
    character_index   INT      NOT NULL     COMMENT 'ĳ���� ID',
    item_type         TINYINT  NOT NULL     COMMENT '������ Type',
    item_index        INT      NOT NULL     COMMENT '������ ID',
    PRIMARY KEY (user_index, character_index, item_index)
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

## User_Summon_State Table

```sql

���� �ε���
Avail �̱� Ƚ��
�� ���� Ƚ��
```