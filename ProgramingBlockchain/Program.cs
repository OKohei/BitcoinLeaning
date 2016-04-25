using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using NBitcoin;

namespace ProgramingBlockchain
{
	class MainClass
	{
		private static String mMyWif = "cNiiraQWjYAkG7vtJ4yTgsUeRLbs6PGwFHRiQnj7GfDhfiAbwJDV";
		private static String mMyBitcoinAddress = "moDDy5F9WYNnkC3jzkqk5qwd7xb47DHSdU";
		private static String mBCHBitcoinAddress = "n4No3YRJExF9RB1XiCmAef9LHUxFJrxeB8";


		public static void Main (string[] args)
		{
			createKeys ();
            //test
		}

		//秘密鍵・ビットコイン鍵の生成
		public static void createKeys()
		{
			Key key = new Key (); //秘密鍵の生成
		
			//Bitcoin Secret(Wallet Important Formatっていいます。WIFと略される)
			BitcoinSecret secret = key.GetBitcoinSecret(Network.TestNet);
			Console.WriteLine("Bitcoin Secret(WIF): {0}", secret);

			PubKey pubKey = key.PubKey; //公開鍵の生成
			Console.WriteLine ("Public Key: {0}", pubKey);

			KeyId hash = pubKey.Hash; //Hash化されたPublic Keyの取得 
			Console.WriteLine ("Hashed public key: {0}", hash);

			BitcoinPubKeyAddress address = pubKey.GetAddress (Network.TestNet);
			Console.WriteLine ("Address: {0}", address);

			Script scriptPubKeyFromAddress = address.ScriptPubKey;
			Console.WriteLine ("ScriptPubKey from address: {0}", scriptPubKeyFromAddress);

			Script scriptPubKeyFromHash = hash.ScriptPubKey;
			Console.WriteLine ("ScriptPubKey from hash: {0}", scriptPubKeyFromHash);
		}

		//ScriptPubkeyからビットコインアドレスを取得する
		public static void getAddressFromScriptPubKey(String ScriptPubKey)
		{
			Script scriptPubKey = new Script(ScriptPubKey);
			BitcoinAddress address = scriptPubKey.GetDestinationAddress(Network.TestNet);
			Console.WriteLine("Bitcoin Address: {0}", address);
		}

		//ScriptHashからビットコインアドレスを取得する
		public static void getAddressFromScriptHash(String ScriptPubKey)
		{
			Script scriptPubKey = new Script("OP_DUP OP_HASH160 1b2da6ee52ac5cd5e96d2964f12a0241851f8d2a OP_EQUALVERIFY OP_CHECKSIG");
			KeyId hash = (KeyId)scriptPubKey.GetDestination();
			Console.WriteLine ("Public Key Hash: {0}", hash);
			BitcoinAddress address = new BitcoinPubKeyAddress(hash, Network.Main);
			Console.WriteLine("Bitcoin Address: {0}", address);
		}

		//トランズアクションを取得してみましょう
		//無料でTestnetのコインが取得できるサイト
		//https://accounts.blockcypher.com/testnet-faucet
		//https://testnet.coinfaucet.eu/en/ (IPアドレスの縛りあり)
		public static void getFundedTransaction(String TxId) 
		{
			var blockr = new BlockrTransactionRepository(Network.TestNet);
			Transaction fundingTransaction = blockr.Get(TxId);
			Console.WriteLine (fundingTransaction);
		}


		//PublicKeyHashにお支払いをしてみましょう (P2PKH)
		public static void PayToPublicKeyHash(String TxId) 
		{
			var blockr = new BlockrTransactionRepository(Network.TestNet);
			Transaction fundingTransaction = blockr.Get(TxId);

			//すでに利用されたトランズアクションのアウトプットから、今回利用するトランズアクションを選択します。
			Transaction payment = new Transaction();
			payment.Inputs.Add(new TxIn()
			{
				PrevOut = new OutPoint(fundingTransaction.GetHash(), 0) //参照するアウトプット(TxOut)のインデックス
			});
						
			// 自分に返ってくるお釣りをいくらするかにし、自分の公開鍵を設定します
			BitcoinAddress myBitcoinAddress = BitcoinAddress.Create(mMyBitcoinAddress, Network.TestNet);
			payment.Outputs.Add (new TxOut () {
				Value = Money.Coins (0.0003m),
				ScriptPubKey = myBitcoinAddress.ScriptPubKey
			});

			// 相手側に送るコインの量と相手側の公開鍵をいれます。
			BitcoinAddress bhcBitcoinAddress = BitcoinAddress.Create(mBCHBitcoinAddress, Network.TestNet);
			payment.Outputs.Add(new TxOut()
			{
				Value = Money.Coins(0.0001m),
				ScriptPubKey = bhcBitcoinAddress.ScriptPubKey
			});

			//利用するインプットにサインをします
			payment.Inputs[0].ScriptSig = myBitcoinAddress.ScriptPubKey;
			payment.Sign(new BitcoinSecret(mMyWif), false);
			Console.WriteLine (payment.ToHex());
		}


		//OpenAssets, Colored Coin等で使われてるOP_RETURNを少し学んでみましょう(時間が余った人or宿題です)
		public static void PayToPublicKeyHashWithOP_RETURN(String TxId) 
		{
			//Colored Coin, OpenAssetsで使われているOP_RETURN(最大値80Bytes)を利用したトランズアクションの生成
			var blockr = new BlockrTransactionRepository(Network.TestNet);
			Transaction fundingTransaction = blockr.Get(TxId);
			Transaction payment = new Transaction();
			payment.Inputs.Add(new TxIn()
			{
				PrevOut = new OutPoint(fundingTransaction.GetHash(), 0) //参照するアウトプット(TxOut)のインデックス
			});

			BitcoinAddress myBitcoinAddress = BitcoinAddress.Create(mMyBitcoinAddress, Network.TestNet);
			payment.Outputs.Add (new TxOut () {
				Value = Money.Coins (0.0003m),
				ScriptPubKey = myBitcoinAddress.ScriptPubKey
			});

			BitcoinAddress bhcBitcoinAddress = BitcoinAddress.Create(mBCHBitcoinAddress, Network.TestNet);
			payment.Outputs.Add(new TxOut()
			{
				Value = Money.Coins(0.0001m),
				ScriptPubKey = bhcBitcoinAddress.ScriptPubKey
			});

			//ここがOP_RETURNを作る部分
			var message = "Thanks ! :)";
			var bytes = Encoding.UTF8.GetBytes(message);
			payment.Outputs.Add(new TxOut()
			{
				Value = Money.Zero,
				ScriptPubKey = TxNullDataTemplate.Instance.GenerateScriptPubKey (bytes)
			});

			payment.Inputs[0].ScriptSig = myBitcoinAddress.ScriptPubKey;
			payment.Sign(new BitcoinSecret(mMyWif), false);
			Console.WriteLine (payment.ToHex());
		}
	}
}
