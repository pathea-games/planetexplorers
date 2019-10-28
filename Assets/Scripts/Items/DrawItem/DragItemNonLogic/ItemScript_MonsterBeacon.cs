using UnityEngine;

// Move monster beacon out of itemDragAgent's management
public class ItemScript_MonsterBeacon : ItemScript
{
    [UnityEngine.SerializeField]
    private int m_monsterBeaconId;

    private EntityMonsterBeacon _entityBcn;
	private DragItemAgent _agent;

	public int MBId { get { return m_monsterBeaconId; } }

	private void DestroySelf()
	{
		DragItemAgent.Destory (_agent);
	}
	private void OnNewWave(AISpawnTDWavesData.TDWaveSpData tdData, int wave)
	{
		if (wave == tdData._waveDatas.Count-1) {
			Invoke("DestroySelf", 10.0f);
		}
	}

	public override void OnActivate()
    {
		base.OnActivate();

		if (_entityBcn != null) {
			Debug.LogError("[MonsterBeaconItem]:MonsterBeacon has existed.");
			return;
		}
        //Put off aispawn tower defense
		int entityId = GameConfig.IsMultiMode ? id : -1;
		_entityBcn = EntityMonsterBeacon.CreateMonsterBeaconByTDID(m_monsterBeaconId, transform, new TowerInfoUIData(), entityId, null, -1, true);
		if(_entityBcn != null){
			_entityBcn.handlerNewWave += OnNewWave;
			_agent = DragItemAgent.GetById(id);
			if (_agent != null){                
				SceneMan.RemoveSceneObj(_agent);	// Not been managed by sceneMan
			}
		}    
    }
    private void OnDestroy()
    {
		if (!GameConfig.IsMultiMode && _entityBcn != null) {
			Pathea.PeCreature.Instance.Destory (_entityBcn.Id);
		}
    }
}