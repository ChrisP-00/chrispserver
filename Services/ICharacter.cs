using chrispserver.ResReqModels;
using SqlKata.Execution;
using System.Data.Common;
using static chrispserver.ResReqModels.Request;

namespace chrispserver.Services;

public interface ICharacter
{
    Task<Result> EquipCharacterAsync(Req_EquipCharacter requestBody);
    Task<Result> EquipItemAsync(Req_EquipItem resquestBody);
    Task<Result> UnequipItemAsnyc(Req_EquipItem resquestBody);
    Task<Result> PlayStatusAsync(Req_PlayStatus requestBody);
    Task<Result> UpdateCharacterExpAsync(Req_Exp requestBody, QueryFactory db, DbTransaction transaction);
}
