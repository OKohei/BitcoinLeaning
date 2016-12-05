using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Numerics;
using NBitcoin;
using NBitcoin.DataEncoders;
using NBitcoin.OpenAsset;
using NBitcoin.Protocol;

namespace ProgramingBlockchain
{
	class MainClass
	{
		private static String mMyWif = "cSCzcyx5kihVwnP1CVRoDzvaqPj9TFXaoc8pSEbeQZ9W1YPkaeQd"; //自分のWif（書き換える）
		private static String mMyBitcoinAddress = "mkxm4FMS2RBkpLaPbpkZmiRMVK3PwgoYi2"; // 自分のアドレス（書き換える）
		private static String mBCHWif = "cVxfm7SsbtAGcL5zhP6aJVbRBVJLiCS2i4EGnL64AJSm7HervuyN";
		private static String mBCHBitcoinAddress = "n4d96BKBGGeyAh9XQRai6GicNTtYTB8D5m";
		private static String mUtXo = "9376b522d961e437c33cdf948112ac69e63faa036d3fcc84a1e67c2cf229ca42"; //自分のUnspent Transaction
		private static int mUtXoIndex = 0;
		private static decimal mChange = 0.5m; //お釣り
		private static decimal mNumTranfer = 0.49m; //送付金額


		// Explore https://live.blockcypher.com/btc-testnet/pushtx/
		// Faucet https://testnet.coinfaucet.eu/en/ (IP制限があります)
		//

		public static void Main (string[] args)
		{
			//Step1: 自分の鍵を作る
			//createKeys();

			//Step2: 送ってみる
			PayToPublicKeyHash(mUtXo);
		}

		//秘密鍵・ビットコイン鍵の生成
		public static void createKeys()
		{
			Key key = new Key (); //秘密鍵の生成
			//Bitcoin Secret(Wallet Import Formatっていいます。WIFと略される)
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

			Script paymentScriptFromAddress = address.ScriptPubKey.PaymentScript;
			Console.WriteLine ("PaymentScript from address: {0}", paymentScriptFromAddress);

			Script scriptPubKeyFromHash = hash.ScriptPubKey;
			Console.WriteLine ("ScriptPubKey from hash: {0}", scriptPubKeyFromHash);
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
				PrevOut = new OutPoint(fundingTransaction.GetHash(), mUtXoIndex) //参照するアウトプット(TxOut)のインデックス
			});
						
			// 自分に返ってくるお釣りをいくらするかにし、自分の公開鍵を設定します
			BitcoinAddress myBitcoinAddress = BitcoinAddress.Create(mMyBitcoinAddress, Network.TestNet);
			payment.Outputs.Add (new TxOut () {
				Value = Money.Coins (mChange),
				ScriptPubKey = myBitcoinAddress.ScriptPubKey
			});

			// 相手側に送るコインの量と相手側の公開鍵をいれます。
			BitcoinAddress bhcBitcoinAddress = BitcoinAddress.Create(mBCHBitcoinAddress, Network.TestNet);
			payment.Outputs.Add(new TxOut()
			{
				Value = Money.Coins(mNumTranfer),
				ScriptPubKey = bhcBitcoinAddress.ScriptPubKey
			});

			//利用するインプットにサインをします
			payment.Inputs[0].ScriptSig = myBitcoinAddress.ScriptPubKey;
			payment.Sign(new BitcoinSecret(mMyWif), false);
			Console.WriteLine (payment.ToHex());
		}

		//ScriptPubkeyからビットコインアドレスを取得する
		public static void getAddressFromScriptPubKey(String ScriptPubKey)
		{
			Script scriptPubKey = new Script(ScriptPubKey);
			BitcoinAddress address = scriptPubKey.GetDestinationAddress(Network.TestNet);
			Console.WriteLine("Bitcoin Address: {0}", address);
		}

		//ScriptHashからビットコインアドレスを取得する
		public static BitcoinAddress getAddressFromScriptHash(String scriptHash)
		{
			Script scriptPubKey = new Script(scriptHash);
			KeyId hash = (KeyId)scriptPubKey.GetDestination();
			Console.WriteLine("Public Key Hash: {0}", hash);
			BitcoinAddress address = new BitcoinPubKeyAddress(hash, Network.TestNet);
			Console.WriteLine("Bitcoin Address: {0}", address);
			return address;
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

		//Multisigを学んでみる
		public static void PayToScriptHash(String TxId) 
		{
			var blockr = new BlockrTransactionRepository(Network.TestNet);
			Transaction fundingTransaction = blockr.Get(TxId);
			Transaction payment = new Transaction();
			payment.Inputs.Add(new TxIn()
			{
				PrevOut = new OutPoint(fundingTransaction.GetHash(), 0) //参照するアウトプット(TxOut)のインデックス
			});

			Key bob = new BitcoinSecret(mMyWif).PrivateKey;
			Key alice = new BitcoinSecret(mBCHWif).PrivateKey;

			//BitcoinAddress myBitcoinAddress = BitcoinAddress.Create(mMyBitcoinAddress, Network.TestNet);
			var scriptPubKey = PayToMultiSigTemplate
				.Instance
				.GenerateScriptPubKey(2, new[] {alice.PubKey,  bob.PubKey});
			//paymentScript.GetScriptAddress
			Console.WriteLine(scriptPubKey);
		}
	}
}
