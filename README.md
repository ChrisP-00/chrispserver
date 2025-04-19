# 🎮 ChrispServer - Game Server API

게임 클라이언트와 통신하는 ASP.NET Core 기반의 게임 서버 API입니다. 

회원가입, 로그인, 캐릭터 장착, 아이템 사용, 미션 진행 등 게임 로직을 REST API로 제공합니다.

## 📦 프로젝트 구조

```plaintext
├── Controllers
│   ├── AccountController.cs
│   ├── CharacterController.cs
│   └── RewardController.cs
│
├── Services
│   ├── AccountService.cs
│   ├── CharacterService.cs
│   ├── MissionService.cs
│   └── MasterHandler.cs
│
├── Middlewares
│   ├── UserAuthMiddleware.cs
│   ├── RedisMemoryDb.cs / InMemoryDb.cs
│
├── Securities
│   ├── RedisAuthService.cs
│   └── RedisKeyMaker.cs
│
├── DbConfigurations
│   └── ConnectionManager.cs
│
├── ResReqModels
│   ├── Request.cs
│   ├── Response.cs
│   └── Result.cs
```


## 🔧 Controller
### AccountController
- POST/Account/CreateAccount : 회원가입
- POST/Account/Login : 로그인

### CharacterController
- POST /Character/EquipCharacter : 캐릭터 변경
- POST /Character/EquipItem : 아이템 장착
- POST /Character/UnequipItem : 아이템 해제
- POST /Character/PlayStatus : 행동 (밥주기, 놀아주기 등)

### RewardController
- POST /Reward/ReceiveMission : 미션 보상 수령 처리


## 🧩 Middleware
### UserAuthMiddleware
- 토큰 유효성 검증 및 TTL 갱신 (로그인 상태 유지)
- Redis 락을 통한 중복 요청 방지 (SETNX)
- 인증 정보 context.Items에 저장

## RedisMemoryDb / InMemoryDb
- 인증 토큰과 사용자 정보 관리
- Redis 혹은 테스트용 In-Memory 선택 가능


## 🚕 Service
### AccountService
- 계정 생성 시 기본 캐릭터, 아이템, 재화, 미션 초기화
- 로그인 시 유저 전체 상태 정보 제공 (캐릭터, 인벤토리, 재화 등)

### CharacterService
- 캐릭터/아이템 장착 및 해제
- 행동 처리 → 재화 차감 → 미션 업데이트 → 경험치 적용

### MissionService
- 미션 진행도 업데이트
- 완료된 미션에 대한 보상 지급 처리

### MasterHandler
- 마스터 데이터 조회 캐시 핸들러
- InfoItem, InfoGoods, InfoLevel 등 타입 기반 조회 제공


---


# 🔐 Redis + Middleware 기반 인증 시스템 (with Cafe24 External Token)

## 📌 목적

- 클라이언트가 Cafe24에서 발급한 외부 토큰을 이용해 로그인할 수 있도록 지원
- 토큰 유효성을 **카페24 API로 먼저 검증**
- 검증된 유저에 한해서만 **서버 전용 토큰을 Redis에 저장 및 발급**
- 이후 요청은 모두 미들웨어에서 Redis 기반 인증 및 중복 요청 제어 처리



## 🧱 시스템 구성요소

| 컴포넌트 | 역할 |
|----------|------|
| `UserAuthMiddleware` | 모든 요청 전에 사용자 인증 및 요청 락 처리 |
| `RedisAuthService` | 서버 전용 토큰 생성/검증/삭제/TTL 연장 |
| `RedisMemoryDb` | 요청 중복 방지를 위한 Redis 기반 락 처리 |
| `Cafe24AuthValidator` | 클라이언트가 준 카페24 토큰의 유효성 검증 (HttpClient 사용) |



## 🔄 전체 인증 흐름

```plaintext
[클라이언트 요청]
   |
   ├── ① Authorization 헤더 확인
   │     → Redis에서 토큰 유효성 검증
   │     → 실패 시 JSON Body에서 추출 시도
   |
   ├── ② Redis에서 memberId/token 비교
   ├── ③ context.Items["AuthUser"] 에 유저 정보 저장
   ├── ④ Redis SETNX로 유저 락 설정 (TTL 3초)
   └── ⑤ 다음 컨트롤러로 요청 전달
```
