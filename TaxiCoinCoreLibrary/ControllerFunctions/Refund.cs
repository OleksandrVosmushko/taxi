using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using Newtonsoft.Json;
using System;
using TaxiCoinCoreLibrary.RequestObjectPatterns;
using TaxiCoinCoreLibrary.TokenAPI;
using TaxiCoinCoreLibrary.Utils;

namespace TaxiCoinCoreLibrary.ControllerFunctions
{
    public class Refund
    {
        public static string Create(UInt64 id, DefaultControllerPattern req, User user)
        {
            user.PublicKey = EthECKey.GetPublicAddress(user.PrivateKey);
            TransactionReceipt result;
            try
            {
                result = TokenFunctionsResults<int>.InvokeByTransaction(user, FunctionNames.Refund, req.Gas, funcParametrs: id);
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(e.Message);
            }

            return JsonConvert.SerializeObject(result);
        }

        public static string Approve(UInt64 id, DefaultControllerPattern req, User user)
        {
            user.PublicKey = EthECKey.GetPublicAddress(user.PrivateKey);
            TransactionReceipt result;
            try
            {
                result = TokenFunctionsResults<int>.InvokeByTransaction(user, FunctionNames.ApproveRefund, req.Gas, funcParametrs: id);
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(e.Message);
            }

            return JsonConvert.SerializeObject(result);
        }

        public static string DisApprove(UInt64 id, DefaultControllerPattern req, User user)
        {
            user.PublicKey = EthECKey.GetPublicAddress(user.PrivateKey);
            TransactionReceipt result;
            try
            {
                result = TokenFunctionsResults<int>.InvokeByTransaction(user, FunctionNames.DisApproveRefund, req.Gas, funcParametrs: id);
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(e.Message);
            }

            return JsonConvert.SerializeObject(result);
        }
    }
}
