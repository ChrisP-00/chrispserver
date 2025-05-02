using chrispserver.ResReqModels;
using chrispserver.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using static chrispserver.ResReqModels.Request;

namespace chrispserver.Controllers;

[ApiController]
[Route("Character")]
public class CharacterController : ControllerBase
{
    private readonly ICharacter _character;

    public CharacterController(ICharacter character)
    {
        _character = character;
    }

    /// <summary>
    /// 캐릭터 장착
    /// </summary>
    [HttpPost("EquipCharacter")]
    public async Task<Result> EquipCharacterAsync([FromBody]Req_EquipCharacter requestBody)
    {
        Console.WriteLine(">>>>>>>>>>>>>>>> [Equip-Character Request] " + JsonSerializer.Serialize(requestBody));
        return await _character.EquipCharacterAsync(requestBody);
    }

    /// <summary>
    /// 아이템 장착
    /// </summary>
    [HttpPost("EquipItem")]
    public async Task<Result> EquipItemAsync([FromBody] Req_EquipItem requestBody)
    {
        Console.WriteLine(">>>>>>>>>>>>>>>> [Equip-Item Request] " + JsonSerializer.Serialize(requestBody));
        return await _character.EquipItemAsync(requestBody);
    }

    /// <summary>
    /// 아이템 장탈
    /// </summary>
    [HttpPost("UnequipItem")]
    public async Task<Result> UnequipItemByTypeAsnyc([FromBody] Req_EquipItem requestBody)
    {
        Console.WriteLine(">>>>>>>>>>>>>>>> [Unequip-Item Request] " + JsonSerializer.Serialize(requestBody));

        return await _character.UnequipItemAsnyc(requestBody);
    }

    /// <summary>
    /// 유저 캐릭터 플레이 상태 업데이트
    /// </summary>
    [HttpPost("PlayStatus")]
    public async Task<Result> PlayStatusAsync([FromBody] Req_PlayStatus requestBody)
    {
        if (!ModelState.IsValid)
        {
            Console.WriteLine("[ModelState] 바인딩 실패");
            foreach (var kvp in ModelState)
            {
                foreach (var error in kvp.Value.Errors)
                {
                    Console.WriteLine($"[Error] {kvp.Key}: {error.ErrorMessage}");
                }
            }

            return Result.Fail(ResultCodes.InputData_MissingRequiredField);
        }

        Console.WriteLine(">>>>>>>>>>>>>>>> [PlayStatus Request] " + JsonSerializer.Serialize(requestBody));

        Console.WriteLine($"[DEBUG] PlayStatus 요청 받음: userIndex = {requestBody.UserIndex}, goodType = {requestBody.GoodType}");

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

        return await _character.PlayStatusAsync(requestBody);
    }
}
