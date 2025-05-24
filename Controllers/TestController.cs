using chrispserver.DbConfigurations;
using chrispserver.ResReqModels;
using chrispserver.Services;
using Microsoft.AspNetCore.Mvc;
using SqlKata.Execution;
using System.Text.RegularExpressions;
using static chrispserver.DbEntity.InfoEntities;
using static chrispserver.ResReqModels.Request;
using static chrispserver.ResReqModels.Response;

namespace chrispserver.Controllers;
[ApiController]
[Route("Test")]
public class TestController : ControllerBase
{
    private readonly IMasterHandler _masterHandler;
    private readonly IAccount _account;
    private readonly ConnectionManager _connectionManager;

    public TestController(IMasterHandler masterHandler, IAccount account, ConnectionManager connectionManager)
    {
        _masterHandler = masterHandler;
        _account = account;
        _connectionManager = connectionManager;
    }

    [HttpGet("GetAllAccounts")]
    public async Task<Result<List<User_Account>>> GetAllAccounts()
    {
        try
        {
            var db = _connectionManager.GetSqlQueryFactory(DbKeys.GameServerDB);
            var users = await db.Query("user_account").GetAsync<User_Account>();

            return Result<List<User_Account>>.Success(users.ToList());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetAllAccounts] 예외 발생: {ex}");
            return Result<List<User_Account>>.Fail(ResultCodes.TestFailException, ex.Message);
        }
    }

    public class Req_DeleteAccounts
    {
        public List<int> UserIndexes { get; set; } = new();
    }

    [HttpPost("deleteAccounts")]
    public async Task<Result> DeleteAccouns([FromBody] Req_DeleteAccounts req)
    {
        if (req.UserIndexes == null || req.UserIndexes.Count == 0)
            return Result.Fail(ResultCodes.InValidRequestHttpBody);

        var db = _connectionManager.GetSqlQueryFactory(DbKeys.GameServerDB);

        foreach(var userIndex in req.UserIndexes)
        {
            await db.Query("user_character").Where("user_index", userIndex).DeleteAsync();
            await db.Query("user_goods").Where("user_index", userIndex).DeleteAsync();
            await db.Query("user_inventory").Where("user_index", userIndex).DeleteAsync();
            await db.Query("user_equip").Where("user_index", userIndex).DeleteAsync();
            await db.Query("user_daily_mission").Where("user_index", userIndex).DeleteAsync();
            await db.Query("user_account").Where("user_index", userIndex).DeleteAsync();
        }

        return Result.Success();
    }

    [HttpGet("GetUserById")]
    public async Task<Result<Res_UserWithGoods>> GetUserById([FromQuery] int userIndex)
    {
        var db = _connectionManager.GetSqlQueryFactory(DbKeys.GameServerDB);

        var account = await db.Query("user_account").Where("user_index", userIndex).FirstOrDefaultAsync<User_Account>();
        var goods = await db.Query("user_goods").Where("user_index", userIndex).GetAsync<User_Goods>();

        if (account == null)
            return Result<Res_UserWithGoods>.Fail(ResultCodes.No_Account);

        return Result<Res_UserWithGoods>.Success(new Res_UserWithGoods
        {
            user_Index = account.User_Index,
            nickname = account.Nickname,
            user_Goods = goods.ToList()
        });
    }


    [HttpGet("Characters")]
    public IActionResult GetCharacters()
    {
        var characters = _masterHandler.GetAll<InfoCharacter>() ?? new();
        return Ok(new { data = characters });
    }

    /// <summary>
    /// 닉네임 변경 - test 
    /// </summary>
    [HttpPost("UpdateNickname")]
    public async Task<Result> UpdateNickname([FromBody] Req_UpdateNickname req)
    {
        if (string.IsNullOrWhiteSpace(req.Nickname) ||
            req.Nickname.Length > 10 ||
            !Regex.IsMatch(req.Nickname, "^[a-zA-Z0-9가-힣]+$") ||  // 특수문자 금지
            req.UserIndex <= 0)
        {
            return Result.Fail(ResultCodes.InValidRequestHttpBody);
        }

        var db = _connectionManager.GetSqlQueryFactory(DbKeys.GameServerDB);

        int affected = await db.Query("user_account")
            .Where("user_index", req.UserIndex)
            .UpdateAsync(new { nickname = req.Nickname });

        if (affected == 0)
            return Result.Fail(ResultCodes.Account_Update_Fail);

        return Result.Success();
    }




    /// <summary>
    /// 재화 추가 - test 
    /// </summary>
    [HttpPost("AddGoods")]
    public async Task<Result> AddGoods([FromBody] Req_AddGoods req)
    {
        if (req.UserIndex <= 0 || req.GoodsIndex <= 0 || req.Quantity <= 0)
            return Result.Fail(ResultCodes.InValidRequestHttpBody);

        var db = _connectionManager.GetSqlQueryFactory(DbKeys.GameServerDB);

        // 기존 값 증가 또는 새로 추가
        int affected = await db.Query("user_goods")
            .Where("user_index", req.UserIndex)
            .Where("goods_index", req.GoodsIndex)
            .IncrementAsync("quantity", req.Quantity);

        if (affected == 0)
        {
            await db.Query("user_goods").InsertAsync(new
            {
                user_index = req.UserIndex,
                goods_index = req.GoodsIndex,
                quantity = req.Quantity
            });
        }

        return Result.Success();
    }
}
