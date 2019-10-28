using WhiteCat;

namespace Pathea
{
    public class DragCreationLodCmpt : PeCmpt
    {
		public void Construct(DragItemLogicCreation dragLogic)
        {
			var creation = GetComponent<CreationController>();
			creation.visible = true;
			
			switch (creation.category)
			{
				case EVCCategory.cgAircraft:
                    {
                        ItemScript itemScript = GetComponent<ItemScript>();
                        if (null != itemScript)
                        {
                            itemScript.SetItemObject(dragLogic.itemDrag.itemObj);
                            itemScript.InitNetlayer(dragLogic.mNetlayer);
                            itemScript.id = dragLogic.id;
                        }
                        break;
                    }
                case EVCCategory.cgBoat:
				case EVCCategory.cgVehicle:
				case EVCCategory.cgRobot:
				case EVCCategory.cgAITurret:
					{
						ItemScript itemScript = GetComponent<ItemScript>();
						if (null != itemScript)
						{
							itemScript.SetItemObject(dragLogic.itemDrag.itemObj);
							itemScript.InitNetlayer(dragLogic.mNetlayer);
							itemScript.id = dragLogic.id;
						}
						break;
					}
			}
		}


        public void Destruct(DragItemLogicCreation dragLogic)
        {
			var creation = GetComponent<CreationController>();
			creation.visible = false;
		}


        public void Activate(DragItemLogicCreation dragLogic)
        {
			var controller = GetComponent<BehaviourController>();
            if (controller) controller.physicsEnabled = true;

			var creation = GetComponent<CreationController>();
			creation.collidable = true;

			Entity.SendMsg(EMsg.Lod_Collider_Created);
        }


        public void Deactivate(DragItemLogicCreation dragLogic)
        {
			var creation = GetComponent<CreationController>();
			creation.collidable = false;

			var controller = GetComponent<BehaviourController>();
            if (controller) controller.physicsEnabled = false;

            Entity.SendMsg(EMsg.Lod_Collider_Destroying);
        }
    }
}