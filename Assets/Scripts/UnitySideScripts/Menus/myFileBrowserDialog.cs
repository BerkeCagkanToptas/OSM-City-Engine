using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UnitySideScripts.Menus
{


    class myFileBrowserDialog : MonoBehaviour
    {
        private DirectoryInfo currentDirectory;
        private FileInfo[] files;
        private DirectoryInfo[] folders;

        public Texture2D folderIcon, fileIcon, directoryIcon;
        public BrowserState state;

        public enum BrowserState { Processing, Selected, Cancelled, None};

        public enum BrowserMode { FileSelect, FolderSelect }
        private string[] acceptedExtensions;
        private BrowserMode mode;


        private InputField IFpath;
        private InputField IFsaveName;
        private Transform contentPanel;
        private GameObject previousItem;
        private string _path;
        private string _saveName;

        public string selectedPath
        {
            get { return _path;}
            set { if(IFpath != null)
                  IFpath.text = shortenString(value); _path = value; }
        }
        public string saveName
        {
            get { return _saveName;  }
            set { _saveName = value; }
        }

        void Start()
        {
            IFpath = transform.Find("Panel").Find("IFpath").GetComponent<InputField>();
            IFsaveName = transform.Find("Panel").Find("EnterName").Find("IFfileName").GetComponent<InputField>();
            contentPanel = transform.Find("Panel").Find("ScrollRect").Find("Content Panel");           
        }


        private void fillList()
        {
            if(IFpath == null)
                IFpath = transform.Find("Panel").Find("IFpath").GetComponent<InputField>();
            if(contentPanel == null)
                contentPanel = transform.Find("Panel").Find("ScrollRect").Find("Content Panel");

            folders = currentDirectory.GetDirectories().Where(d=> d.Name[0] != '.').ToArray();
            var allowedExtensions = new HashSet<string>(acceptedExtensions, StringComparer.OrdinalIgnoreCase);
            files = currentDirectory.GetFiles().Where(f => allowedExtensions.Contains(f.Extension) && f.Name[0] != '.').ToArray();

            selectedPath = currentDirectory.FullName;

            var children = new List<GameObject>();
            foreach (Transform child in contentPanel) children.Add(child.gameObject);
            children.ForEach(child => Destroy(child));
           
            for (int i = 0; i < folders.Length; i++)
            {
                GameObject Item = GameObject.Instantiate((GameObject)Resources.Load("Prefabs/Menu/FileItem"));
                RawImage icon = Item.transform.Find("Panel").Find("RIicon").GetComponent<RawImage>();
                Text filename = Item.transform.Find("Panel").Find("TextFileName").GetComponent<Text>();
                Toggle directoryFlag = Item.transform.Find("Panel").Find("IsDirectory").GetComponent<Toggle>();
                Text index = Item.transform.Find("Panel").Find("Index").GetComponent<Text>();
                index.text = i.ToString();
                Item.GetComponent<Button>().onClick.AddListener(delegate { currentDirectory = folders[int.Parse(index.text)]; fillList(); });

                icon.texture = folderIcon;
                filename.text = folders[i].Name + folders[i].Extension;
                directoryFlag.isOn = true;
                Item.transform.SetParent(contentPanel);              
            }
            for (int i = 0 ; i < files.Length ; i++)
            {
                GameObject Item = GameObject.Instantiate((GameObject)Resources.Load("Prefabs/Menu/FileItem"));
                RawImage icon = Item.transform.Find("Panel").Find("RIicon").GetComponent<RawImage>();
                Text filename = Item.transform.Find("Panel").Find("TextFileName").GetComponent<Text>();
                Toggle directoryFlag = Item.transform.Find("Panel").Find("IsDirectory").GetComponent<Toggle>();
                Text index = Item.transform.Find("Panel").Find("Index").GetComponent<Text>();
                index.text = i.ToString();
  
                Item.GetComponent<Button>().onClick.AddListener(delegate { onFileClick(files[int.Parse(index.text)], Item); });
                icon.texture = fileIcon;
                filename.text = files[i].Name; //+ files[i].Extension;
                directoryFlag.isOn = false;
                Item.transform.SetParent(contentPanel);              
            }


        }

        public void onFileClick(FileInfo selectedFile, GameObject selectedItem)
        {
            selectedPath = selectedFile.FullName;
            if(previousItem != null)
                previousItem.GetComponent<Button>().interactable = true;
            selectedItem.GetComponent<Button>().interactable = false;
            previousItem = selectedItem;
        }

        public void onCancelClick()
        {
            state = BrowserState.Cancelled;
            selectedPath = "";
            gameObject.SetActive(false);
            Debug.Log("<Color=red>PATH: </Color>" + "NONE");
        }

        public void onSelectClick()
        {
            state = BrowserState.Selected;
            gameObject.SetActive(false);
            Debug.Log("<Color=red>PATH: </Color>" + selectedPath);
        }

        public void onBackClick()
        {
            currentDirectory = currentDirectory.Parent;
            fillList();
        }

        public void onSaveNameChanged()
        {
            saveName = IFsaveName.text;
        }
        
        public void draw(BrowserMode _mode, DirectoryInfo startingPath,string[] _acceptedExtensions)
        {
            state = BrowserState.Processing;
            gameObject.SetActive(true);
            if (_mode == BrowserMode.FolderSelect)
                gameObject.transform.Find("Panel").Find("EnterName").gameObject.SetActive(true);
            else
                gameObject.transform.Find("Panel").Find("EnterName").gameObject.SetActive(false);
            mode = _mode;
            currentDirectory = startingPath;
            acceptedExtensions = _acceptedExtensions;
            fillList();
        }

        private string shortenString(string original)
        {
            int maxCharacter = 40;

            if (original.Length < maxCharacter)
                return original;

            else
            {
                string[] subStrings = original.Split(new char[] { '/', '\\' });

                string shortenedString = "";

                for(int k = subStrings.Length-1 ; k > 0 ; k--)
                {                    
                    string tmpString = "/" + subStrings[k] + shortenedString;
                    if (tmpString.Length > maxCharacter)
                    {
                        shortenedString = "/..." + shortenedString;
                        break;
                    }
                    else
                        shortenedString = tmpString;
                }

                return shortenedString;
            }

        }

    }
}
