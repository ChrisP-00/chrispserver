namespace chrispserver.DbConfigurations;

public static class DbColumns
{
    #region 공용 컬럼
    /// <summary> 수량 </summary>
    public const string Quantity = "quantity";
    /// <summary> 명칭 </summary>
    public const string Name = "name";
    /// <summary> 설명 </summary>
    public const string Description = "description";
    /// <summary> 값 </summary>
    public const string Value = "value";
    /// <summary> 생성 날짜 </summary>
    public const string CreatedAt = "created_at";
    /// <summary> 로그인 날짜 </summary>
    public const string LastLoginAt = "last_login_at";
    /// <summary> 장착한 날짜 </summary>
    public const string EquippedAt = "equipped_at";
    #endregion

    #region 공지 사항 테이블 컬럼
    /// <summary> 공지사항의 고유 인덱스 </summary>
    public const string NoticeIndex = "notice_index";
    /// <summary> 공지 제목 </summary>
    public const string Title = "Title";
    /// <summary> 공지 내용 </summary>
    public const string Content = "Content";
    #endregion

    #region 서버 버전 테이블 컬럼
    /// <summary> 앱 버전 </summary>
    public const string AppVersion = "app_version";
    /// <summary> 점검 여부 </summary>
    public const string IsMaintenance = "is_maintenance";
    /// <summary> 점검 시작 시간 </summary>
    public const string MaintenanceStartAt = "maintenance_start_at";
    /// <summary> 점검 종료 시간 </summary>
    public const string MaintenanceEndAt = "maintenance_end_at";
    #endregion

    #region 유저 정보 테이블 컬럼
    /// <summary> 사용자 인덱스 </summary>
    public const string UserIndex = "user_index";
    /// <summary> 맴버 아이디 </summary>
    public const string MemberId = "member_id";
    /// <summary> 닉네임 </summary>
    public const string Nickname = "nickname";
    #endregion

    #region 유저 재화 테이블 컬럼
    /// <summary> 재화 인덱스 </summary>
    public const string GoodsIndex = "goods_index";
    /// <summary> 재화 타입 </summary>
    public const string Goods_Type = "goods_type";
    #endregion

    #region 캐릭터 테이블 컬럼
    /// <summary> 캐릭터 인덱스 </summary>
    public const string CharacterIndex = "character_index";
    /// <summary> 캐릭터 레벨 </summary>
    public const string CharacterLevel = "level";
    /// <summary> 캐릭터 인덱스 </summary>
    public const string IsActive = "is_active";
    #endregion

    #region 아이템 테이블 컬럼
    /// <summary> 아이템 인덱스 </summary>
    public const string ItemIndex = "item_index";
    /// <summary> 아이템 장착 </summary>
    public const string IsEquipped = "is_equipped";
    /// <summary> 유형 </summary>
    public const string ItemType = "item_type";
    #endregion

    #region 미션 테이블 컬럼
    /// <summary> 미션 인덱스 </summary>
    public const string DailyMissionIndex = "daily_mission_index";
    /// <summary> 미션 진행도 </summary>
    public const string Mission_progress = "mission_progress";
    /// <summary> 미션 목표 수 </summary>
    public const string Mission_Goal_Count = "mission_gaol_count";
    #endregion
}
