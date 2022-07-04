using System;
using System.Numerics;
using Cysharp.Threading.Tasks;
using MoralisUnity;
using Nethereum.Hex.HexTypes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NFT_Minter
{
    public class MintingPanel : MonoBehaviour
    {
        [Header("Smart Contract Data")]
        [SerializeField] private string contractAddress;
        [SerializeField] private string contractAbi;
        [SerializeField] private string contractFunction;

        private BigInteger _currentTokenId;

        [Header("NFT Metadata")]
        [SerializeField] private string metadataUrl;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI metadataUrlText;
        [SerializeField] private Button mintButton;
        [SerializeField] private Button openSeaButton;
        [SerializeField] private TextMeshProUGUI statusText;
        
        private void OnEnable()
        {
            statusText.text = string.Empty;
            metadataUrlText.text = metadataUrl;
        }

        private void OnDisable()
        {
            statusText.text = string.Empty;
            metadataUrlText.text = string.Empty;
        }

        private void OnValidate()
        {
            metadataUrlText.text = metadataUrl;
        }

        #region MINTING_METHODS

        public async void MintNft()
        {
            if (contractAddress == string.Empty || contractAbi == string.Empty || contractFunction == string.Empty)
            {
                Debug.LogError("Contract data is not fully set");
                return;
            }

            if (metadataUrl == string.Empty)
            {
                Debug.LogError("Metadata URL is empty");
                return;
            }
            
            statusText.text = "Please confirm transaction in your wallet";
            mintButton.interactable = false;
        
            var result = await ExecuteMinting(metadataUrl);

            if (result is null)
            {
                statusText.text = "Transaction failed";
                mintButton.interactable = true;
                return;
            }
    
            // We tell the GameManager what we minted the item successfully
            statusText.text = "Transaction completed!";
            Debug.Log($"Token Contract Address: {contractAddress}");
            Debug.Log($"Token ID: {_currentTokenId}");
            
            // Activate OpenSea button
            mintButton.gameObject.SetActive(false);
            openSeaButton.gameObject.SetActive(true);
        }
    
        private async UniTask<string> ExecuteMinting(string tokenUrl)
        {
            // Dummy TokenId based on current time.
            long currentTime = DateTime.Now.Ticks;
            _currentTokenId = new BigInteger(currentTime);
        
            // These are the parameters that the contract function expects
            object[] parameters = {
                _currentTokenId.ToString("x"), // This is the format the contract expects
                tokenUrl
            };

            // Set gas configuration. If you set it at 0, your wallet will use its default gas configuration
            HexBigInteger value = new HexBigInteger(0);
            HexBigInteger gas = new HexBigInteger(0);
            HexBigInteger gasPrice = new HexBigInteger(0);

            string resp = await Moralis.ExecuteContractFunction(contractAddress, contractAbi, contractFunction, parameters, value, gas, gasPrice);

            return resp;
        }

        #endregion


        #region SECONDARY_METHODS

        public void ViewContract()
        {
            if (contractAddress == string.Empty)
            {
                Debug.Log("Contract address is not set");
                return;
            }
            
            MoralisTools.Web3Tools.ViewContractOnPolygonScan(contractAddress);
        }

        public void ViewOnOpenSea()
        {
            MoralisTools.Web3Tools.ViewNftOnTestnetOpenSea(contractAddress, Moralis.CurrentChain.Name, _currentTokenId.ToString());
        }

        #endregion
    }   
}
