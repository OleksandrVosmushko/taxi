using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using TaxiCoinCoreLibrary.RequestObjectPatterns;
using TaxiCoinCoreLibrary.TokenAPI;
using TaxiCoinCoreLibrary.Utils;

namespace TaxiCoinCoreLibrary.ControllerFunctions
{
    public class Payment
    {
        public static async Task<string> GetById(UInt64 id, User user)
        {
            user.PublicKey = EthECKey.GetPublicAddress(user.PrivateKey);
            ContractFunctions contractFunctions = Globals.GetInstance().ContractFunctions;
            TokenAPI.Payment res;
            try
            {
                res = await contractFunctions.DeserializePaymentById(user.PublicKey, user.PrivateKey, id);
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(e.Message);
            }

            return JsonConvert.SerializeObject(res);
        }

        public static string Create(UInt64 id, CreatePaymentPattern req, User user)
        {
            user.PublicKey = EthECKey.GetPublicAddress(user.PrivateKey);
            TransactionReceipt result;
            try
            {
                result = TokenFunctionsResults<int>.InvokeByTransaction(user, FunctionNames.CreatePayment, req.Gas, new object[] { id, req.Value });
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(e.Message);
            }

            return JsonConvert.SerializeObject(result);
        }
    }
}
