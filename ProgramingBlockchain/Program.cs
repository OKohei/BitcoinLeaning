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
		private static String mMyWif = "cSCzcyx5kihVwnP1CVRoDzvaqPj9TFXaoc8pSEbeQZ9W1YPkaeQd";
		private static String mMyBitcoinAddress = "mkxm4FMS2RBkpLaPbpkZmiRMVK3PwgoYi2";
		private static String mBCHWif = "cVxfm7SsbtAGcL5zhP6aJVbRBVJLiCS2i4EGnL64AJSm7HervuyN";
		private static String mBCHBitcoinAddress = "n4d96BKBGGeyAh9XQRai6GicNTtYTB8D5m";


		public static void Main (string[] args)
		{
			String txId = "736546da79447960368456bcb9581a86245f6e103497d4670e7973e2b752fcb0";
			PayToPublicKeyHash(txId);
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
			Console.WriteLine ("Public Key Hash: {0}", hash);
			BitcoinAddress address = new BitcoinPubKeyAddress(hash, Network.TestNet);
			Console.WriteLine("Bitcoin Address: {0}", address);
			return address;
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
				PrevOut = new OutPoint(fundingTransaction.GetHash(), 3) //参照するアウトプット(TxOut)のインデックス
			});
						
			// 自分に返ってくるお釣りをいくらするかにし、自分の公開鍵を設定します
			BitcoinAddress myBitcoinAddress = BitcoinAddress.Create(mMyBitcoinAddress, Network.TestNet);
			payment.Outputs.Add (new TxOut () {
				Value = Money.Coins (1.0m),
				ScriptPubKey = myBitcoinAddress.ScriptPubKey
			});

			// 相手側に送るコインの量と相手側の公開鍵をいれます。
			BitcoinAddress bhcBitcoinAddress = BitcoinAddress.Create(mBCHBitcoinAddress, Network.TestNet);
			payment.Outputs.Add(new TxOut()
			{
				Value = Money.Coins(0.36820972m),
				ScriptPubKey = bhcBitcoinAddress.ScriptPubKey
			});

			//利用するインプットにサインをします
			payment.Inputs[0].ScriptSig = myBitcoinAddress.ScriptPubKey;
			payment.Sign(new BitcoinSecret(mMyWif), false);
			Console.WriteLine("GELLKHALKSFJLKDJF");
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


		//Bitcoin Address Arbitary
		public static void Arbitary(String TxId) 
		{
			BitcoinAddress address = BitcoinAddress.Create("1KF8kUVHK42XzgcmJF4Lxz4wcL5WDL97PB");
			var birth = Encoding.UTF8.GetBytes("18/07/1988");
			var birthHash = NBitcoin.Crypto.Hashes.Hash256(birth);
			Script redeemScript = new Script("OP_IF " + "OP_HASH256 " + Op.GetPushOp(birthHash.ToBytes()) + " OP_EQUAL " + "OP_ELSE " + address.ScriptPubKey + " " + "OP_ENDIF");
			Console.WriteLine (redeemScript);
//			var tx = new Transaction();
//			tx.Outputs.Add(new TxOut(Money.Parse("0.0001"), redeemScript.Hash));
//			ScriptCoin scriptCoin = tx.Outputs.AsCoins().First().ToScriptCoin(redeemScript);
//
//			Transaction spending = new Transaction();
//			spending.AddInput(new TxIn(new OutPoint(tx, 0)));
//
//			Op pushBirthdate = Op.GetPushOp(birth);
//			Op selectIf = OpcodeType.OP_1; //go to if
//			Op redeemBytes = Op.GetPushOp(redeemScript.ToBytes());
//			Script scriptSig = new Script(pushBirthdate, selectIf, redeemBytes);
//			spending.Inputs[0].ScriptSig = scriptSig;
//
//			var result = spending
//				.Inputs
//				.AsIndexedInputs()
//				.First()
//				.VerifyScript(tx.Outputs[0].ScriptPubKey);
//			Console.WriteLine(result); // True
		}

		public static void witness()
		{
		}

		public static void coloredCoin()
		{

			BitcoinAddress btcAddress = BitcoinAddress.Create(mMyBitcoinAddress, Network.TestNet);
			var coin = new Coin(
				fromTxHash: new uint256("eb49a599c749c82d824caf9dd69c4e359261d49bbb0b9d6dc18c59bc9214e43b"),
				fromOutputIndex: 0,
				amount: Money.Satoshis(2000000),
				scriptPubKey: new Script(Encoders.Hex.DecodeData(btcAddress.ScriptPubKey.ToHex())));

			var issuance = new IssuanceCoin(coin);

			var nico = BitcoinAddress.Create(mMyBitcoinAddress);
			var bookKey = new BitcoinSecret(mMyWif);
			TransactionBuilder builder = new TransactionBuilder();

			var tx = builder
				.AddKeys(bookKey)
				.AddCoins(issuance)
				.IssueAsset(nico, new AssetMoney(issuance.AssetId, quantity: 255))
				.SendFees(Money.Coins(0.0001m))
				.SetChange(bookKey.GetAddress())
				.BuildTransaction(true);

			Console.WriteLine(tx);
		}


	}
}
