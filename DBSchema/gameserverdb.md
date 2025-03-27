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
    user_index                  INT          NOT NULL AUTO_INCREMENT PRIMARY KEY COMMENT '유저 고유 ID',
    member_id                   VARCHAR(20)  NOT NULL   COMMENT '카페24 멤버 아이디',
    unity_device_number         VARCHAR(100) NOT NULL   COMMENT '유니티 디바이스 ID',
    nickname                    VARCHAR(15)  NOT NULL   COMMENT '닉네임',
    is_banned                   BOOLEAN      NOT NULL DEFAULT FALSE     COMMENT '계정 정지 여부: FALSE=정상, TRUE=정지됨',
    is_delete                   BOOLEAN      NOT NULL DEFAULT FALSE     COMMENT '계정 삭제 여부: FALSE=정상, TRUE=삭제됨',
    created_at                  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP     COMMENT '계정 생성 시각 (KST)',
    last_login_at               DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP     COMMENT '마지막 로그인 시각 (KST)'
) COMMENT='유저 계정 정보 테이블';
```

## User_Character Table

``` sql
DROP TABLE IF EXISTS gameserverdb.`user_character`;
CREATE TABLE IF NOT EXISTS gameserverdb.`user_character` (
    user_index	                INT	         NOT NULL   COMMENT '사용자 고유 식별자',
    character_index	            INT	         NOT NULL   COMMENT '캐릭터 고유 식별자',
    level	                    INT	         NOT NULL DEFAULT 0     COMMENT '캐릭터 레벨',
    exp	                        INT	         NOT NULL DEFAULT 0     COMMENT '캐릭터 경험치',
    quantity	                INT	         NOT NULL DEFAULT 1     COMMENT '캐릭터 보유 수량',
    is_active	                BOOL         NOT NULL DEFAULT FALSE     COMMENT	'현재 키우는 중인지 여부 (0 = 아님, 1 = 키우는 중)',
    owned_at	                DATETIME DEFAULT CURRENT_TIMESTAMP	    COMMENT	'캐릭터 소유 날짜	',
    PRIMARY KEY (user_index, character_index)
) COMMENT='유저 캐릭터 정보 테이블';
```

## User_Goods Table

``` sql
DROP TABLE IF EXISTS gameserverdb.`user_goods`;
CREATE TABLE IF NOT EXISTS gameserverdb.`user_goods` (
    user_index	                INT	        NOT NULL    COMMENT '사용자 고유 식별자',
    goods_index	                TINYINT     NOT NULL    COMMENT '재화 고유 식별자',
    quantity	                INT		    NOT NULL DEFAULT 0  COMMENT	'보유 개수',
    PRIMARY KEY (user_index, goods_index)
) COMMENT='유저 재화 테이블';
```


## User_Invetory Table

```sql
DROP TABLE IF EXISTS gameserverdb.`user_inventory`;
CREATE TABLE IF NOT EXISTS gameserverdb.`user_inventory` (
    user_index                  INT         NOT NULL    COMMENT '사용자 고유 식별자',
    item_index                  INT         NOT NULL    COMMENT '아이템 고유 식별자',
    owned_at                    DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP    COMMENT '아이템 획득 날짜',
    PRIMARY KEY (user_index, item_index)
) COMMENT='유저 보유 아이템 테이블';
```


## User_Equip Table

```sql
DROP TABLE IF EXISTS gameserverdb.`user_equip`;
CREATE TABLE IF NOT EXISTS gameserverdb.`user_equip` (
    user_index        INT      NOT NULL     COMMENT '사용자 고유 식별자',
    character_index   INT      NOT NULL     COMMENT '캐릭터 ID',
    item_index        INT      NOT NULL     COMMENT '아이템 ID',
    is_equipped       BOOL     NOT NULL DEFAULT FALSE      COMMENT '착용 여부 (0 = 미착용, 1 = 착용)',
    PRIMARY KEY (user_index, character_index, item_index)
) COMMENT='유저 캐릭터 장착 테이블';
```


## User_Daily_Mission Table

```sql
DROP TABLE IF EXISTS gameserverdb.`user_daily_mission`;
CREATE TABLE IF NOT EXISTS gameserverdb.`user_daily_mission` (
    user_index          INT NOT NULL    COMMENT '사용자 고유 식별자',
    mission_index       INT NOT NULL    COMMENT '일일 미션 고유 인덱스',
    amount              INT NOT NULL DEFAULT 0  COMMENT '수행 횟수',
    is_received         BOOL NOT NULL DEFAULT FALSE COMMENT '수령 여부',
    received_at         DATETIME    COMMENT '수령 완료 날짜 - 날이 지나면 초기화',
    PRIMARY KEY (user_index, mission_index)
) COMMENT='유저 일일 미션 테이블';
```

## User_Character_Mission Table

```sql
DROP TABLE IF EXISTS gameserverdb.`user_character_mission`;
CREATE TABLE IF NOT EXISTS gameserverdb.`user_character_mission` (
    user_index          INT NOT NULL    COMMENT '사용자 고유 식별자',
    mission_index       INT NOT NULL    COMMENT '캐릭터 미션 고유 인덱스',
    quantity            INT NOT NULL    COMMENT '수행 횟수',
    is_received         BOO NOT NULL DEFAULT FALSE  COMMENT '수령 여부',
    received_at         DATETIME   COMMENT '수령 완료 날짜',
    PRIMARY KEY (user_index, mission_index)
) COMMENT='캐릭터 미션 테이블';
```

## User_Summon_State Table

```sql

유저 인덱스
Avail 뽑기 횟수
총 뽑은 횟수
```