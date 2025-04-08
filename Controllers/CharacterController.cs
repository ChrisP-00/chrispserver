using chrispserver.ResReqModels;
using chrispserver.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using static chrispserver.DbEntity.InfoEntities;
using static chrispserver.ResReqModels.Request;

namespace chrispserver.Controllers;

[ApiController]
[Route("Character")]
public class CharacterController : ControllerBase
{
    private readonly ICharacter _character;
    private readonly IMission _mission;
    private readonly IMasterHandler _masterHandler;

    public CharacterController(ICharacter character, IMission mission, IMasterHandler masterHandler)
    {
        _character = character;
        _mission = mission;
        _masterHandler = masterHandler;
    }

    /// <summary>
    /// 캐릭터 장착
    /// </summary>
    [HttpPost("Equip-Character")]
    public async Task<Result> EquipCharacterAsync([FromBody]Req_EquipCharacter requestBody)
    {
        if(_masterHandler.IsValid<InfoCharacter>(requestBody.EquipCharacterIndex))
        {
            Console.WriteLine($"Db에 존재하지 않는 캐릭터 인덱스 : {requestBody.EquipCharacterIndex}");
            Result.Fail(ResultCodes.Equip_Fail_CharacterNotExist);
        }
        
        Console.WriteLine(">>>>>>>>>>>>>>>> [Equip-Character Request] " + JsonSerializer.Serialize(requestBody));
        return await _character.EquipCharacterAsync(requestBody);
    }

    /// <summary>
    /// 아이템 장착
    /// </summary>
    [HttpPost("Equip-Item")]
    public async Task<Result> EquipItemrAsync([FromBody] Req_EquipItem requestBody)
    {
        InfoItem infoItem = _masterHandler.GetInfoDataByIndex<InfoItem>(requestBody.EquipItemIndex);
       
        if (infoItem == null)
        {
            Console.WriteLine($"Db에 존재하지 않는 아이템 인덱스 : {requestBody.EquipItemIndex}");
            return Result.Fail(ResultCodes.Equip_Fail_NotExist);
        }

        Console.WriteLine(">>>>>>>>>>>>>>>> [Equip-Item Request] " + JsonSerializer.Serialize(requestBody));
        return await _character.EquipItemAsync(requestBody, infoItem);
    }

    /// <summary>
    /// 아이템 장탈
    /// </summary>
    [HttpPost("Unequip-Item")]
    public async Task<Result> UnequipItemByTypeAsnyc([FromBody] Req_EquipItem requestBody)
    {
        Console.WriteLine(">>>>>>>>>>>>>>>> [Unequip-Item Request] " + JsonSerializer.Serialize(requestBody));

        return await _character.UnequipItemAsnyc(requestBody);
    }

    /// <summary>
    /// 유저 재화 소비
    /// </summary>
    [HttpPost("UseGoods")]
    public async Task<Result> UseGoodsAsync([FromBody] Req_UseGoods requestBody)
    {
        Console.WriteLine(">>>>>>>>>>>>>>>> [Feed Request] " + JsonSerializer.Serialize(requestBody));

        if (_masterHandler.IsValid<InfoItem>(requestBody.GoodsIndex))
        {
            Console.WriteLine($"Db에 존재하지 않는 아이템 인덱스 : {requestBody.GoodsIndex}");
            Result.Fail(ResultCodes.Equip_Fail_NotExist);
        }

        if (!Enum.IsDefined(requestBody.GoodType))
        {
            Console.WriteLine("Good Type에 정의되지 않은 Type");
            return Result.Fail(ResultCodes.Goods_Fail_NotValidType);
        }

        if (requestBody.Quantity <= 0)
        {
            Console.WriteLine("수량이 0보다 작음");
            return Result.Fail(ResultCodes.Goods_Fail_LessThanZero);
        }

        var result = await _character.UseGoodsAsync(requestBody);

        if (result.ResultCodes == ResultCodes.Ok)
        {
            result = await _mission.UpdateMissionProcessAsync(requestBody.UserIndex, requestBody.GoodsIndex, requestBody.Quantity);
        }

        return result;
    }
}
