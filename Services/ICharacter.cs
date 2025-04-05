using chrispserver.ResReqModels;
using Microsoft.AspNetCore.Mvc;
using static chrispserver.DbEntity.InfoEntities;
using static chrispserver.ResReqModels.Request;

namespace chrispserver.Services;

public interface ICharacter
{
    
    Task<Result> UseGoodsAsync(Req_UseGoods requestBody);
    
    Task<Result> EquipCharacterAsync(Req_EquipCharacter requestBody);

    Task<Result> EquipItemAsync(Req_EquipItem resquestBody, InfoItem infoItem);

    Task<Result> UnequipItemAsnyc(Req_EquipItem resquestBody);
}
