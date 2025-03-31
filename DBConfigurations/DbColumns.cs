namespace chrispserver.DbConfigurations;

public static class DbColumns
{
    #region 공용 컬럼
    /// <summary> 유형 </summary>
    public const string Type = "type";
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
    #endregion

    #region 캐릭터 테이블 컬럼
    /// <summary> 캐릭터 인덱스 </summary>
    public const string CharacterIndex = "character_index";

    /// <summary> 캐릭터 인덱스 </summary>
    public const string IsActive = "is_active";
    #endregion

    #region 아이템 테이블 컬럼
    /// <summary> 캐릭터 인덱스 </summary>
    public const string ItemIndex = "item_index";
    #endregion
}
