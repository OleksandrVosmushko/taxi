using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using Newtonsoft.Json;
using System;
using TaxiCoinCoreLibrary.RequestObjectPatterns;
using TaxiCoinCoreLibrary.TokenAPI;
using TaxiCoinCoreLibrary.Utils;

namespace TaxiCoinCoreLibrary.ControllerFunctions
{
    public class Order
    {
        public static string GetOrder(UInt64 id, DefaultControllerPattern req, User user)
        {
            user.PublicKey = EthECKey.GetPublicAddress(user.PrivateKey);
            TransactionReceipt result;
            try
            {
                result = TokenFunctionsResults<TransactionReceipt>.InvokeByTransaction(user, FunctionNames.GetOrder, req.Gas, funcParametrs: id);
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(e.Message);
            }

            return JsonConvert.SerializeObject(result);
        }

        public static TransactionReceipt CompleteOrder(UInt64 id, DefaultControllerPattern req, User user)
        {
            user.PublicKey = EthECKey.GetPublicAddress(user.PrivateKey);
            TransactionReceipt result;
            result = TokenFunctionsResults<int>.InvokeByTransaction(user, FunctionNames.CompleteOrder, req.Gas, funcParametrs: id);
            return result;
        }
    }
}
