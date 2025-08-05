using System;
using System.Collections.Generic;
using Item;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class ItemUI : MonoBehaviour, IPointerClickHandler
    {
        //[Header("Slot")]
        private static float PanelSlotPadding => GameManager.Instance.CellSize * 3; //CellSize * 3
        public static float CellSize => GameManager.Instance.CellSize;

        [SerializeField] private Color availableColor;
        [SerializeField] private Color unavailableColor;
        
        [SerializeField] private Image quickSlot04;
        [SerializeField] private TMP_Text quickSlot04Count;
        [SerializeField] private Image quickSlot05;
        [SerializeField] private TMP_Text quickSlot05Count;
        [SerializeField] private Image quickSlot06;
        [SerializeField] private TMP_Text quickSlot06Count;
        [SerializeField] private Image quickSlot07;
        [SerializeField] private TMP_Text quickSlot07Count;
        private readonly Dictionary<int, (Image img, TMP_Text txt)> _quickSlotDict = new();
    
        [Header("Player Inventory")]
        [Header("Left Panel")]
        [SerializeField] private RectTransform leftPanel;
        [SerializeField] private RectTransform headwearSlot;
        [SerializeField] private RectTransform eyewearSlot;
        [SerializeField] private RectTransform bodyArmorSlot;
        [SerializeField] private RectTransform primaryWeaponSlot;
        [SerializeField] private RectTransform secondaryWeaponSlot;
        [SerializeField] private RectTransform leftPanelItemParentRT;
        public RectTransform HeadwearSlotRT => headwearSlot;
        public RectTransform EyewearSlotRT => eyewearSlot;
        public RectTransform BodyArmorSlotRT => bodyArmorSlot;
        public RectTransform PWeaponSlotRT => primaryWeaponSlot;
        public RectTransform SWeaponSlotRT => secondaryWeaponSlot;
    
        [Header("Middle Panel")]
        [SerializeField] private RectTransform middlePanel;
        [SerializeField] private RectTransform chestRigSlot;
        [SerializeField] private RectTransform chestRigParent;
        [SerializeField] private RectTransform rigInvenParent;
        [SerializeField] private RectTransform rigItemRT;
        private GameObject _rigSlotInstance;
        [SerializeField] private RectTransform backpackSlot;
        [SerializeField] private RectTransform backpackParent;
        [SerializeField] private RectTransform packInvenParent;
        [SerializeField] private RectTransform backpackItemRT;
        private GameObject _backpackSlotInstance;
        [SerializeField] private float minMiddlePanelItemHeight = 250f;
        [SerializeField, Space] private RectTransform pocketsParent;
        [SerializeField] private List<RectTransform> pockets = new();
        [SerializeField] private RectTransform pocketsItemRT;
    
        public RectTransform RigSlotRT => chestRigSlot;
        public RectTransform BackpackSlotRT => backpackSlot;
        public List<RectTransform> PocketsSlotRT => pockets;
        public RectTransform RigInvenParent => rigInvenParent;
        public RectTransform BackpackInvenParent => packInvenParent;

        [Header("Right Panel")]
        [SerializeField] private TMP_Text lootCrateName;
        [SerializeField] private RectTransform rightPanel;
        [SerializeField] private RectTransform lootSlotParent;
        [SerializeField] private float minLootSlotHeight = 800f;
        public RectTransform LootSlotParent => lootSlotParent;
    
        private GameObject _lootSlotInstance;
    
        [SerializeField, Space] private Image slotAvailable;
        
        [SerializeField,Space] private RectTransform itemContextMenu;
        [SerializeField] private Button itemContextInfo;
        [SerializeField] private Button itemContextUse;
        [SerializeField] private Button itemContextEquip;
        [SerializeField] private Button itemContextDrop;
        public event Action<ItemContextType> OnItemContextMenuClick;
        public event Action OnCloseItemContextMenu;

        [SerializeField, Space] private RectTransform itemInfoMenu;
        [SerializeField] private TMP_Text itemInfoNameTxt;
        [SerializeField] private Button itemInfoCloseBtn;
        [SerializeField] private Image itemInfoIcon;
        [SerializeField] private TMP_Text itemInfoDescriptionTxt;
        
        private readonly List<RectTransform> _gearSlotRT = new();
        private readonly List<RectTransform> _inventoriesRT = new();
        
        private void Awake()
        {
            //RectTransform Init
            _gearSlotRT.Add(headwearSlot);
            _gearSlotRT.Add(eyewearSlot);
            _gearSlotRT.Add(bodyArmorSlot);
            _gearSlotRT.Add(primaryWeaponSlot);
            _gearSlotRT.Add(secondaryWeaponSlot);
            _gearSlotRT.Add(chestRigSlot);
            _gearSlotRT.Add(backpackSlot);
    
            foreach (var pocketRT in pockets)
            {
                _gearSlotRT.Add(pocketRT);
            }
        
            _inventoriesRT.Add(rigInvenParent);
            _inventoriesRT.Add(packInvenParent);
            _inventoriesRT.Add(lootSlotParent);
            
            //QuickSlot
            _quickSlotDict.Add(4, (quickSlot04, quickSlot04Count));
            _quickSlotDict.Add(5, (quickSlot05, quickSlot05Count));
            _quickSlotDict.Add(6, (quickSlot06, quickSlot06Count));
            _quickSlotDict.Add(7, (quickSlot07, quickSlot07Count));
        }

        private void OnEnable()
        {
            //ItemContextMenu
            itemContextInfo.onClick.AddListener(() => OnItemContextMenu(ItemContextType.Info));
            itemContextUse.onClick.AddListener(() => OnItemContextMenu(ItemContextType.Use));
            itemContextEquip.onClick.AddListener(() => OnItemContextMenu(ItemContextType.Equip));
            itemContextDrop.onClick.AddListener(() => OnItemContextMenu(ItemContextType.Drop));
            
            //ItemInfoMenu
            itemInfoCloseBtn.onClick.AddListener(CloseItemInfo);
            
            CloseItemContextMenu();//ContextMenu닫기
            CloseItemInfo(); //아이템 설명창 끄기
        }

        private void OnDisable()
        {
            //ItemContextMenu
            itemContextInfo.onClick.RemoveListener(() => OnItemContextMenu(ItemContextType.Info));
            itemContextUse.onClick.RemoveListener(() => OnItemContextMenu(ItemContextType.Use));
            itemContextEquip.onClick.RemoveListener(() => OnItemContextMenu(ItemContextType.Equip));
            itemContextDrop.onClick.RemoveListener(() => OnItemContextMenu(ItemContextType.Drop));
            
            //ItemInfoMenu
            itemInfoCloseBtn.onClick.RemoveListener(CloseItemInfo);
            
            
        }
        
        private void OnItemContextMenu(ItemContextType contextType)
        {
            OnItemContextMenuClick?.Invoke(contextType);
            OnCloseItemContextMenu?.Invoke();
        }
        
        public void ShowSlotAvailable(bool isAvailable, Vector2 position, Vector2 size)
        {
            var slotColor = isAvailable ? availableColor : unavailableColor;
            slotAvailable.enabled = true;
            slotAvailable.color = slotColor;
            slotAvailable.rectTransform.position = position;
            slotAvailable.rectTransform.sizeDelta = size;
        }

        public void ClearShowAvailable()
        {
            slotAvailable.enabled = false;
        }
    
        public (RectTransform matchSlot, bool isGearSlot) GetItemSlotRT(Vector2 mousePos)
        {
            foreach (var slot in _gearSlotRT)
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(slot, mousePos)) continue;
                return (slot, true);
            }

            foreach (var inventory in _inventoriesRT)
            {
                if(!RectTransformUtility.RectangleContainsScreenPoint(inventory, mousePos)) continue;
                return (inventory, false);
            }
            return (null,  false);
        }
        
        public Inventory SetInventorySlot(GameObject inventoryGO, GearType itemType, Guid instanceID, bool isInit)
        {
            //초기화...
            if (!inventoryGO) return null;
        
            Inventory inventory = null;
            float slotPrefabHeight;
        
            switch (itemType)
            {
                case GearType.ArmoredRig:
                case GearType.UnarmoredRig:
                {
                    if (isInit)
                    {
                        _rigSlotInstance = Instantiate(inventoryGO, rigInvenParent);
                        _rigSlotInstance.TryGetComponent(out inventory);
                        inventory.Init(CellSize, instanceID);
                    }
                    else
                    {
                        _rigSlotInstance = inventoryGO;
                        _rigSlotInstance.TryGetComponent(out inventory);
                        _rigSlotInstance.SetActive(true);
                        _rigSlotInstance.transform.SetParent(rigInvenParent);
                    }
                
                    slotPrefabHeight = inventory.Height;
                    
                    rigInvenParent.sizeDelta = new Vector2(inventory.Width, inventory.Height);
                    if (slotPrefabHeight > minMiddlePanelItemHeight)
                    {
                        chestRigParent.sizeDelta = 
                            new Vector2(chestRigParent.sizeDelta.x, slotPrefabHeight + PanelSlotPadding);
                    }
                    else
                    {
                        chestRigParent.sizeDelta = new Vector2(chestRigParent.sizeDelta.x, minMiddlePanelItemHeight);
                    }
                    break;
                }
                case GearType.Backpack:
                {
                    if (isInit)
                    {
                        _backpackSlotInstance = Instantiate(inventoryGO, packInvenParent);
                        _backpackSlotInstance.TryGetComponent(out inventory);
                        inventory.Init(CellSize, instanceID);
                    }
                    else
                    {
                        _backpackSlotInstance = inventoryGO;
                        _backpackSlotInstance.TryGetComponent(out inventory);
                        _backpackSlotInstance.SetActive(true);
                        _backpackSlotInstance.transform.SetParent(packInvenParent);
                    }
                    
                    slotPrefabHeight = inventory.Height;
                    
                    packInvenParent.sizeDelta = new Vector2(inventory.Width, inventory.Height);
                    if (slotPrefabHeight > minMiddlePanelItemHeight)
                    {
                        backpackParent.sizeDelta = 
                            new Vector2(backpackParent.sizeDelta.x, slotPrefabHeight + PanelSlotPadding);
                    }
                    else
                    {
                        backpackParent.sizeDelta = new Vector2(backpackParent.sizeDelta.x, minMiddlePanelItemHeight);
                    }
                    break; 
                }

                case GearType.None:
                {
                    _lootSlotInstance = Instantiate(inventoryGO, lootSlotParent);
                    _lootSlotInstance.TryGetComponent(out inventory);
                    inventory.Init(CellSize, instanceID);
                    slotPrefabHeight = inventory.Height + PanelSlotPadding;
                    
                    lootSlotParent.sizeDelta = new Vector2(inventory.Width, inventory.Height);
                    if (slotPrefabHeight > minLootSlotHeight)
                    {
                        lootSlotParent.sizeDelta = new Vector2(lootSlotParent.sizeDelta.x, slotPrefabHeight);
                    }
                    else
                    {
                        lootSlotParent.sizeDelta = new Vector2(lootSlotParent.sizeDelta.x, minLootSlotHeight);
                    }
                    break;
                }
                    
            }
            return inventory;
        }

        public void SetLootInventory(LootCrate lootCrate)
        {
            var lootInventory = lootCrate?.GetLootInventory();
            if (!lootInventory)
            {
                lootCrateName.text = string.Empty;
                return;
            }
            
            if (_lootSlotInstance)
            {
                _lootSlotInstance.SetActive(false);
            }

            lootCrateName.text = lootCrate.CrateName;
            _lootSlotInstance = lootInventory.gameObject;
            _lootSlotInstance.transform.SetParent(lootSlotParent);
            _lootSlotInstance.transform.localPosition = Vector3.zero;
            _lootSlotInstance.SetActive(true);
            float slotPrefabHeight = lootInventory.Height + PanelSlotPadding;
                    
            lootSlotParent.sizeDelta = new Vector2(lootInventory.Width, lootInventory.Height);
            if (slotPrefabHeight > minLootSlotHeight)
            {
                lootSlotParent.sizeDelta = new Vector2(lootSlotParent.sizeDelta.x, slotPrefabHeight);
            }
            else
            {
                lootSlotParent.sizeDelta = new Vector2(lootSlotParent.sizeDelta.x, minLootSlotHeight);
            }
            
        }
        
        
        public void SetQuickSlot(int idx, bool isStackable, Sprite itemSprite, int count)
        {
            _quickSlotDict[idx].img.sprite = itemSprite;
            _quickSlotDict[idx].img.enabled = true;
            if (!isStackable) return;
            _quickSlotDict[idx].txt.enabled = true;
            _quickSlotDict[idx].txt.text = count.ToString();
        }

        public void UpdateQuickSlotCount(int idx, int count)
        {
            _quickSlotDict[idx].txt.text = count.ToString();
        }

        public void ClearQuickSlot(int idx)
        {
            _quickSlotDict[idx].img.enabled = false;
            _quickSlotDict[idx].txt.enabled = false;
        }

        public void OpenItemContextMenu(Vector3 pos, bool isAvailable, bool isGear)
        {
            itemContextMenu.gameObject.SetActive(true);
            if (isGear)
            {
                itemContextEquip.gameObject.SetActive(true);
                itemContextUse.gameObject.SetActive(false);
                itemContextEquip.interactable = isAvailable;
            }
            else
            {
                itemContextEquip.gameObject.SetActive(false);
                itemContextUse.gameObject.SetActive(true);
                itemContextUse.interactable = isAvailable;
            }
            itemContextMenu.transform.position = pos;
        }

        public void CloseItemContextMenu()
        {
            itemContextMenu.gameObject.SetActive(false);
        }
        
        public void OnPointerClick(PointerEventData eventData) //ClickEvent
        {
            if (!itemContextMenu.gameObject.activeSelf) return; 
            OnCloseItemContextMenu?.Invoke();//ContextMenu가 enable이면 클릭으로 닫기
        }

        public void OpenItemInfo(ItemInstance item) //다른 방법?
        {
            itemInfoMenu.localPosition = Vector3.zero;
            itemInfoMenu.gameObject.SetActive(true);
          
            //ItemInfo Set...
            itemInfoNameTxt.text = item.ItemData.ItemName;
            itemInfoIcon.sprite = item.ItemData.ItemSprite;
            //item type...
            var infoTxt = "";
            var gearType = item.ItemData.GearType;
            switch (gearType)
            {
                case GearType.ArmoredRig or GearType.BodyArmor or GearType.HeadWear or GearType.EyeWear:
                {
                    var gearData = item.ItemData as GearData;
                    infoTxt += $"방어도: {gearData?.ArmorAmount}";
                    break;
                }
                case GearType.Weapon:
                {
                    var weaponData = item.ItemData as WeaponData;
                    if(item is not WeaponInstance weapon || !weaponData) return; //null이면 return
                
                    infoTxt += $"탄종: {EnumManager.AmmoCaliberToString(weaponData.AmmoCaliber)}\n"; //개선(enum)
                    infoTxt += $"장탄: {weapon.CurrentMagazineCount} ";
                    infoTxt += $"탄창 크기: {weaponData.DefaultMagazineSize}\n";
                    infoTxt += $"RPM: {weaponData.RPM}";
                    break;
                }
                case GearType.None:
                {
                    if (item.IsStackable)
                    {
                        infoTxt += $"수량: {item.CurrentStackAmount} ";
                        infoTxt += $"최대 수량: {item.MaxStackAmount}\n";
                    }

                    switch (item.ItemData)
                    {
                        case AmmoData ammoData:
                            infoTxt += $"탄종: {EnumManager.AmmoCaliberToString(ammoData.AmmoCaliber)}\n";
                            //세부 스탯...
                            break;
                        case IConsumableItem consumableItem:
                            //infoTxt += $"체력 회복량: {medicalData.HealAmount}\n";
                            //상태 이상 회복 등
                            break;
                    }
                    break;
                }
            }
            
            itemInfoDescriptionTxt.text = infoTxt;
        }

        private void CloseItemInfo()
        {
            itemInfoMenu.gameObject.SetActive(false);
        }
    }
}
