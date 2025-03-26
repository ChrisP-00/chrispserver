# [ Master DB ]
  
## Database Creation

```sql
DROP DATABASE IF EXISTS masterdb;
CREATE DATABASE IF NOT EXISTS masterdb;
```

## Define Table 

```sql
DROP TABLE IF EXISTS masterdb.`define`;
CREATE TABLE IF NOT EXISTS masterdb.`define` (
    define_index    INT         NOT NULL    COMMENT '���� ���� �ε���',
    description          VARCHAR(20) DEFAULT NULL COMMENT '����',
    value           FLOAT   NOT NULL DEFAULT 0     COMMENT '��',
    PRIMARY KEY (define_index)
) COMMENT='�⺻ ���� ���̺�';
```


## Goods Table

```sql
DROP TABLE IF EXISTS masterdb.`goods`;
CREATE TABLE IF NOT EXISTS masterdb.`goods` (
    goods_index     TINYINT         NOT NULL    COMMENT '��ȭ ���� �ĺ��� (1 = ����, 2 = �峭��, 3 = ����Ʈ ���)',
    name            VARCHAR(20)     DEFAULT NULL COMMENT '��ȭ �̸�',
    PRIMARY KEY (goods_index)
) COMMENT='��� ��ȭ�� ���� ���� ���̺�';
```


## Character Table
```sql
DROP TABLE IF EXISTS masterdb.`character`;
CREATE TABLE IF NOT EXISTS masterdb.`character` (
    character_index     INT             NOT NULL    COMMENT 'ĳ���� ���� �ε���',
    name                VARCHAR(10)     DEFAULT NULL COMMENT 'ĳ���� �̸� (������� ����)',
    PRIMARY KEY (character_index)
) COMMENT='���� �� ��� ĳ������ �⺻ ���� ���̺�';
```


## Item Table
```sql
DROP TABLE IF EXISTS masterdb.`item`;
CREATE TABLE IF NOT EXISTS masterdb.`item` (
    item_index              INT             NOT NULL    COMMENT '��ǰ ���� �ĺ���',
    name               VARCHAR(100)    DEFAULT NULL COMMENT '��ǰ �̸�',
    type               TINYINT         NOT NULL COMMENT '������ Ÿ�� (���� ���� ��)',
    required_level          INT             NOT NULL DEFAULT 0 COMMENT '��� ������ �ּ� ĳ���� ����',
    equip_character_index   INT             NOT NULL COMMENT '���� ������ ĳ���� �ε���',
    PRIMARY KEY (item_index)
) COMMENT='���� �� ��� ������ �⺻ ���� ���̺�';
```


## Daily_Mission Table
```sql
DROP TABLE IF EXISTS masterdb.`daily_mission`;
CREATE TABLE IF NOT EXISTS masterdb.`daily_mission` (
    daily_mission_index     INT         NOT NULL    COMMENT '���� �̼� ���� �ε���',
    type                    INT         NOT NULL COMMENT '�̼� Ÿ�� (1: ���ֱ�, 2: ����ֱ� ��)',
    goods_index             INT         NOT NULL COMMENT '����ؾ� �ϴ� ��ȭ �ε���',
    amount                  TINYINT     NOT NULL COMMENT '���� Ƚ��',
    reward_type             INT         NOT NULL COMMENT '���� Ÿ�� (goods_index ����)',
    reward_amount           INT         NOT NULL COMMENT '���� ����',
    PRIMARY KEY (daily_mission_index)
) COMMENT='���� �̼� ���� ���̺�';
```


## Character_Mission Table

```sql
DROP TABLE IF EXISTS masterdb.`character_mission`;
CREATE TABLE IF NOT EXISTS masterdb.`character_mission` (
    character_mission_index     INT         NOT NULL    COMMENT 'ĳ���� �̼� ���� �ε���',
    character_index	            INT	        NOT NULL   COMMENT 'ĳ���� ���� �ĺ���',
    daily_mission_index         INT         NOT NULL COMMENT '����� ���� �̼� �ε���',
    amount                      TINYINT     NOT NULL COMMENT '���� Ƚ��',
    reward_type                 INT         NOT NULL COMMENT '���� Ÿ�� (goods_index ����)',
    reward_amount               INT         NOT NULL COMMENT '���� ����',
    PRIMARY KEY (character_mission_index)
) COMMENT='ĳ���� ���� �̼� ���� ���̺�';
```
