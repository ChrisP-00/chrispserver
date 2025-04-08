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
    define_index         INT            NOT NULL            COMMENT '정의 고유 인덱스',
    description          VARCHAR(20)    DEFAULT NULL        COMMENT '설명',
    value                FLOAT          NOT NULL DEFAULT 0  COMMENT '값',
    PRIMARY KEY (define_index)
) COMMENT='기본 정의 테이블';
```

## Character Table
```sql
DROP TABLE IF EXISTS masterdb.`info_character`;
CREATE TABLE IF NOT EXISTS masterdb.`info_character` (
    character_index     INT             NOT NULL        COMMENT '캐릭터 고유 인덱스',
    character_name      VARCHAR(10)     DEFAULT NULL    COMMENT '캐릭터 이름 (변경되지 않음)',
    PRIMARY KEY (character_index)
) COMMENT='게임 내 모든 캐릭터의 기본 정보 테이블';
```

## Daily_Mission Table
```sql
DROP TABLE IF EXISTS masterdb.`info_daily_mission`;
CREATE TABLE IF NOT EXISTS masterdb.`info_daily_mission` (
    daily_mission_index     INT         NOT NULL    COMMENT '일일 미션 고유 인덱스',
    goods_type              INT         NOT NULL    COMMENT '미션 타입 (1: 밥주기, 2: 놀아주기 등)',
    goods_index             INT         NOT NULL    COMMENT '사용해야 하는 재화 인덱스',
    mission_goal_count      TINYINT     NOT NULL     COMMENT '미션 수행 목표 수',
    reward_type             TINYINT     NOT NULL    COMMENT '보상 타입 (goods_index 참조)',
    reward_amount           TINYINT     NOT NULL    COMMENT '보상 수량',
    PRIMARY KEY (daily_mission_index)
) COMMENT='일일 미션 정보 테이블';
```

## Goods Table

```sql
DROP TABLE IF EXISTS masterdb.`info_goods`;
CREATE TABLE IF NOT EXISTS masterdb.`info_goods` (
    goods_index         INT           NOT NULL      COMMENT '재화 고유 식별자',
    goods_type          INT           NOT NULL      COMMENT '재화 종류 (1 = 간식, 2 = 장난감, 3 = 뽑기, 4 = 포인트 등등)',   
    goods_name          VARCHAR(20)   DEFAULT NULL  COMMENT '재화 이름',
    PRIMARY KEY (goods_index)
) COMMENT='모든 재화에 대한 정보 테이블';
```

## Item Table
```sql
DROP TABLE IF EXISTS masterdb.`info_item`;
CREATE TABLE IF NOT EXISTS masterdb.`info_item` (
    item_index              INT            NOT NULL            COMMENT '소품 고유 식별자',
    item_name               VARCHAR(100)   DEFAULT NULL        COMMENT '소품 이름',
    item_type               TINYINT        NOT NULL            COMMENT '아이템 타입 (장착 부위 등)',
    required_level          INT            NOT NULL DEFAULT 0  COMMENT '사용 가능한 최소 캐릭터 레벨',
    equip_character_index   INT            NOT NULL            COMMENT '장착 가능한 캐릭터 인덱스',
    PRIMARY KEY (item_index)
) COMMENT='게임 내 모든 아이템 기본 정보 테이블';
```
