using UnityEngine;
using System.Collections.Generic;
using Pathea;
namespace TrainingScene
{
    public class UseItemTask : MonoBehaviour
    {
        static UseItemTask mInstance = null;
        public static UseItemTask Instance { get { return mInstance; } }

        void Awake()
        {
            mInstance = this;
        }

        void OnDestroy()
        {
            if(GameUI.Instance != null &&GameUI.Instance.mItemPackageCtrl != null)
                GameUI.Instance.mItemPackageCtrl.e_OnOpenPackage -= OnOpenPackage;

            if(UIMainMidCtrl.Instance != null)
                UIMainMidCtrl.Instance.e_OnDropItemTask -= OnUIMainMidDropItem;

            if(PeCreature.Instance != null && PeCreature.Instance.mainPlayer != null)
                PeCreature.Instance.mainPlayer.equipmentCmpt.changeEventor.Unsubscribe(OnEquip);
        }

        public void InitOpenPackageScene()
        {
            if (GameUI.Instance != null && GameUI.Instance.mItemPackageCtrl != null)
                GameUI.Instance.mItemPackageCtrl.e_OnOpenPackage += OnOpenPackage;
            if (TrainingTaskManager.Instance != null)
                TrainingTaskManager.Instance.CloseMissionArrow();
        }

        void OnOpenPackage()
        {
            GameUI.Instance.mItemPackageCtrl.e_OnOpenPackage -= OnOpenPackage;
            TrainingTaskManager.Instance.CompleteMission();
        }

        public void InitPutMedScene()
        {
            if (UIMainMidCtrl.Instance != null)
                UIMainMidCtrl.Instance.e_OnDropItemTask += OnUIMainMidDropItem;
            if (TrainingTaskManager.Instance != null)
                TrainingTaskManager.Instance.CloseMissionArrow();
        }

        void OnUIMainMidDropItem()
        {
            if (UIMainMidCtrl.Instance != null)
                UIMainMidCtrl.Instance.e_OnDropItemTask -= OnUIMainMidDropItem;
            TrainingTaskManager.Instance.CompleteMission();
        }

        public void InitEquipKnifeScene()
        {
            if (TrainingTaskManager.Instance != null)
                TrainingTaskManager.Instance.CloseMissionArrow();

            PeCreature.Instance.mainPlayer.equipmentCmpt.changeEventor.Subscribe(OnEquip);
        }

        void OnEquip(object sender, EquipmentCmpt.EventArg arg) 
        {
            if (!arg.isAdd)
                return;
            if (arg.itemObj.protoData.weaponInfo == null)
                return;
            TrainingTaskManager.Instance.CompleteMission();
            PeCreature.Instance.mainPlayer.equipmentCmpt.changeEventor.Unsubscribe(OnEquip);
        }
    }
}
