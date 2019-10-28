using UnityEngine;
using System.Collections;
using PeMap;
using Pathea;

namespace PeUIMap
{
	public class UIRandomMap : UIMap 
	{
		Vector2 mapSize { get{ return RandomMapConfig.Instance.MapSize; } } 

		float mapPerPixel { get {return mapSize.x/(texSize*mScale); } }


		public override void OnCreate ()
		{
			base.OnCreate ();
			InitMapTex();
		}

		protected override void InitWindow ()
		{
			base.InitWindow ();

			mScale = 1f;
			mScaleMin = 1f;
//			mMapPos = Vector2.zero;
			mMapPosMin = Vector3.zero;
	
			ChangeScale(0);
			mScaleMin = 1.0f * Screen.width/texSize;
			mMapPosMin.x = (texSize - Screen.width)/2;
			mMapPosMin.y = (texSize - Screen.height)/2;

			mSelectMask.enabled = false;

			mMapPos = GetInitPos();

			mMapPos.x = Mathf.Clamp(mMapPos.x,-mMapPosMin.x,mMapPosMin.x);
			mMapPos.y = Mathf.Clamp(mMapPos.y,-mMapPosMin.y,mMapPosMin.y);

			mMapWnd.transform.localPosition = new Vector3(mMapPos.x, mMapPos.y, -10);

			mMeatSprite.gameObject.SetActive(false);
			mMoneySprite.gameObject.SetActive(true);
		}

        //lz-2016.09.01 只能用货币
        protected override void OpenWarpWnd(UIMapLabel label)
        {
            mMeatSprite.gameObject.SetActive(false);
            mMoneySprite.gameObject.SetActive(true);
            base.OpenWarpWnd(label);
        }

        protected override Vector3 GetUIPos (Vector3 worldPos)
		{
			Vector3 pos = Vector3.zero;
			pos.x = worldPos.x * (texSize / mapSize.x); 
			pos.y = worldPos.z * (texSize / mapSize.y); 
			pos.z = -1;
			return pos;
		}

		protected override Vector3 GetInitPos ()
		{
			if (GameUI.Instance.mMainPlayer != null)
			{
				Vector3 player_pos = GameUI.Instance.mMainPlayer.position;
				Vector3 pos = Vector3.zero;
				pos.x = -player_pos.x * (texSize / mapSize.x); 
				pos.y = -player_pos.z * (texSize / mapSize.y); 
				pos.z = -1;
				
				return pos;
			}
			else
				return Vector3.zero;
		}

        protected override float ConvetMToPx(float m)
        {
            return texSize/mapSize.x*m;
        }

		protected override Vector3 GetWorldPos (Vector2 mousePos)
		{
			Vector3 pos = Vector3.zero;
			pos.x = (mousePos.x - Screen.width/2 - mMapPos.x)*mapPerPixel;
			pos.z = (mousePos.y - Screen.height/2 - mMapPos.y)*mapPerPixel;
			pos.y = 0;
			return pos;
		}


		#region Random MapTile
		[SerializeField] UITexture 	mMapTex;
		Texture2D 					mTex2d;
		//Material 					mTexMat;
		//Material mTexMat;
		int mTexLen {get {return MaskTile.Mgr.Instance.mNumPerSide;}}

		// color
		[SerializeField] Color colUnknow;
		[SerializeField] Color colDefault;
		[SerializeField] Color colGrassLand;
		[SerializeField] Color colForest;
		[SerializeField] Color colDesert;
		[SerializeField] Color colRedstone;
		[SerializeField] Color colRainforest;
		[SerializeField] Color colSea;
        //lz-2016.08.02 新增山区，沼泽，火山地形的颜色
        [SerializeField] Color colMountain;
        [SerializeField] Color colSwamp;
        [SerializeField] Color colCrater;


        float offsetScale = 1; //lz-2016.11.15 偏移缩放
		void InitMapTex()
		{
			mTex2d = new Texture2D(mTexLen,mTexLen);
			mTex2d.filterMode = FilterMode.Point;
			for(int i = 0; i < mTexLen; i++)
			{
				for(int j = 0; j < mTexLen; j++)
					mTex2d.SetPixel(i, j, colUnknow);
			}
			mTex2d.Apply();
			mMapTex.mainTexture = mTex2d;
			//mTexMat = mMapTex.material;
			MaskTile.Mgr.Instance.eventor.Subscribe(ReflashMaskTile);
            //lz-2016.11.11 因为地图的大小不是128的倍数，所以会产生缩放，这里需要把图片的大小对应一下，避免偏移
            int sourceLen= MaskTile.Mgr.Instance.mLength;
            int curlen= MaskTile.Mgr.Instance.mNumPerSide * MaskTile.Mgr.mLenthPerArea;
            offsetScale = (float)curlen / sourceLen;
            float tagetScale = texSize* offsetScale;
            mMapTex.transform.localScale = new Vector3(tagetScale, tagetScale, 1);
        }

		Color GetMaskTileColor(MaskTileType type)
		{
			switch (type)
			{
			case MaskTileType.GrassLand: 
				return colGrassLand;
			case MaskTileType.Forest: 
				return colForest;
			case MaskTileType.Desert: 
				return colDesert;
			case MaskTileType.Redstone: 
				return colRedstone;
			case MaskTileType.Rainforest: 
				return colRainforest;
			case MaskTileType.Sea: 
				return colSea;
            case MaskTileType.Mountain:
                return colMountain;
            case MaskTileType.Swamp:
                return colSwamp;
            case MaskTileType.Crater:
                return colCrater;
			default:
				return colDefault;
			}
		}

		void ReflashMaskTile(object sender,MaskTile.Mgr.Args args)
		{
			if (args.add)
			{
				Color c = GetMaskTileColor((MaskTileType)(MaskTile.Mgr.Instance.Get(args.index).type));
				int forceGroup = MaskTile.Mgr.Instance.Get(args.index).forceGroup;
				if (forceGroup == -1)
					mTex2d.SetPixel(args.index % mTexLen, args.index / mTexLen, c);
				else
                {
                    Color32 col32=ForceSetting.Instance.GetForceColor(forceGroup);
                    Color col=new Color(col32.r/255f,col32.g/255f,col32.b/255f,col32.a/255f);
                    mTex2d.SetPixel(args.index % mTexLen, args.index / mTexLen, col);
                }
			}
			else
				mTex2d.SetPixel(args.index % mTexLen, args.index / mTexLen, colUnknow);

			//Debug.Log( (args.index%mTexLen).ToString() + " " + (args.index / mTexLen).ToString());
			mTex2d.Apply();
		}
		#endregion

		#region LockArea
		[SerializeField] UISprite mSelectMask;
		int mSelectIndex = -1;
		MaskTile mSelectMaskTile = null;

		protected override void UIMapBg_OnClick ()
		{
			base.UIMapBg_OnClick ();
			if(Input.GetMouseButtonUp(0))
				OnSelectMask();
		}

		void OnSelectMask()
		{
			Vector3 worldPos = GetWorldPos(new Vector2(Input.mousePosition.x,Input.mousePosition.y));
			mSelectIndex = MaskTile.Mgr.Instance.GetMapIndex(worldPos);
			MaskTile maskTile = MaskTile.Mgr.Instance.Get(mSelectIndex);

			if (maskTile ==  mSelectMaskTile)
			{
				mSelectMaskTile = null;
				mSelectMask.enabled = false; 
				mSelectIndex = -1;
			}
			else 
			{
				mSelectMaskTile = maskTile;
				mSelectMask.enabled = true;
				float sizePerTile = texSize / mTexLen;
				mSelectMask.transform.localScale = new Vector3(sizePerTile, sizePerTile, 1);
                //lz-2016.11.15 因为地图的大小不是128的倍数，所以会产生缩放，这里需要把图片的大小对应一下，避免偏移
                float x = ((mSelectIndex % mTexLen - mTexLen/2) * texSize / mTexLen  + 0.5f * sizePerTile) * offsetScale;
				float y = ((mSelectIndex / mTexLen - mTexLen/2) * texSize / mTexLen + 0.5f * sizePerTile)* offsetScale;
				mSelectMask.transform.localPosition = new Vector3(x, y, 0);;
			}
			mWarpWnd.SetActive(false);
		}

		void OnLockBtn()
		{
			if ( mSelectIndex != -1 && null != mSelectMaskTile && null != PlayerNetwork.mainPlayer)
				ServerAdministrator.RequestLockArea(mSelectIndex);
		}
		
		void OnUnLockBtn()
		{
			if (mSelectIndex != -1 && null != mSelectMaskTile && null != PlayerNetwork.mainPlayer)
				ServerAdministrator.RequestUnLockArea(mSelectIndex);
		}
		
		void OnResetBtn()
		{
			if (mSelectIndex != -1 && null != mSelectMaskTile && null != PlayerNetwork.mainPlayer)
				ServerAdministrator.RequestClearVoxelData(mSelectIndex);
		}
		
		void OnResetAllBtn()
		{
			if (mSelectIndex != -1 && null != PlayerNetwork.mainPlayer)
				ServerAdministrator.RequestClearAllVoxelData();
		}

		#endregion

	}
	
}