# [ Master DB ]
  
## Database Creation

```sql
DROP DATABASE IF EXISTS masterdb;
CREATE DATABASE IF NOT EXISTS masterdb;
```

## Define Table 

```sql
DROP TABLE IF EXISTS masterdb.`info_define`;
CREATE TABLE IF NOT EXISTS masterdb.`info_define` (
    define_index         INT            NOT NULL            COMMENT '���� ���� �ε���',
    description          VARCHAR(20)    DEFAULT NULL        COMMENT '����',
    value                FLOAT          NOT NULL DEFAULT 0  COMMENT '��',
    PRIMARY KEY (define_index)
) COMMENT='�⺻ ���� ���̺�';
```

## Character Table
```sql
DROP TABLE IF EXISTS masterdb.`info_character`;
CREATE TABLE IF NOT EXISTS masterdb.`info_character` (
    character_index     INT             NOT NULL        COMMENT 'ĳ���� ���� �ε���',
    character_name      VARCHAR(10)     DEFAULT NULL    COMMENT 'ĳ���� �̸� (������� ����)',
    PRIMARY KEY (character_index)
) COMMENT='���� �� ��� ĳ������ �⺻ ���� ���̺�';
```

## Daily_Mission Table
```sql
DROP TABLE IF EXISTS masterdb.`info_daily_mission`;
CREATE TABLE IF NOT EXISTS masterdb.`info_daily_mission` (
    daily_mission_index     INT         NOT NULL    COMMENT '���� �̼� ���� �ε���',
    goods_type              INT         NOT NULL    COMMENT '�̼� Ÿ�� (1: ���ֱ�, 2: ����ֱ� ��)',
    goods_index             INT         NOT NULL    COMMENT '����ؾ� �ϴ� ��ȭ �ε���',
    mission_goal_count      TINYINT     NOT NULL     COMMENT '�̼� ���� ��ǥ ��',
    reward_type             TINYINT     NOT NULL    COMMENT '���� Ÿ�� (goods_index ����)',
    reward_amount           TINYINT     NOT NULL    COMMENT '���� ����',
    PRIMARY KEY (daily_mission_index)
) COMMENT='���� �̼� ���� ���̺�';
```

## Goods Table

```sql
DROP TABLE IF EXISTS masterdb.`info_goods`;
CREATE TABLE IF NOT EXISTS masterdb.`info_goods` (
    goods_index         INT           NOT NULL      COMMENT '��ȭ ���� �ĺ���',
    goods_type          INT           NOT NULL      COMMENT '��ȭ ���� (1 = ����, 2 = �峭��, 3 = �̱�, 4 = ����Ʈ ���)',   
    goods_name          VARCHAR(20)   DEFAULT NULL  COMMENT '��ȭ �̸�',
    PRIMARY KEY (goods_index)
) COMMENT='��� ��ȭ�� ���� ���� ���̺�';
```

## Item Table
```sql
DROP TABLE IF EXISTS masterdb.`info_item`;
CREATE TABLE IF NOT EXISTS masterdb.`info_item` (
    item_index              INT            NOT NULL            COMMENT '��ǰ ���� �ĺ���',
    item_name               VARCHAR(100)   DEFAULT NULL        COMMENT '��ǰ �̸�',
    item_type               TINYINT        NOT NULL            COMMENT '������ Ÿ�� (���� ���� ��)',
    required_level          INT            NOT NULL DEFAULT 0  COMMENT '��� ������ �ּ� ĳ���� ����',
    equip_character_index   INT            NOT NULL            COMMENT '���� ������ ĳ���� �ε���',
    PRIMARY KEY (item_index)
) COMMENT='���� �� ��� ������ �⺻ ���� ���̺�';
```
