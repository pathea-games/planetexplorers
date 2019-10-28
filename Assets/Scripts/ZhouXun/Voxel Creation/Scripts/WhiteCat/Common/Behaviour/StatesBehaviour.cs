using System;

namespace WhiteCat
{
	/// <summary> 状态 </summary>
	public class State
	{
		/// <summary> 进入 </summary>
		public Action onEnter;

		/// <summary> 离开 </summary>
		public Action onExit;

		/// <summary> 更新 </summary>
		public Action<float> onUpdate;
	}


	/// <summary> 状态机组件 </summary>
	public class StatesBehaviour : BaseBehaviour
	{
		// 当前状态
		private State _state;


		// 从进入状态开始的持续时间
		private float _stateTime;
		

		/// <summary> 当前状态 </summary>
		public State state
		{
			get { return _state; }
			set
			{
				if (_state != null && _state.onExit != null) _state.onExit();
				_stateTime = 0;
				_state = value;
				if (_state != null && _state.onEnter != null) _state.onEnter();
			}
		}


		/// <summary> 从进入状态开始的持续时间 </summary>
		public float stateTime { get { return _stateTime; } }


		/// <summary> 更新状态（在 FixedUpdate，Update 或 LateUpdate 中调用） </summary>
		protected void UpdateState(float deltaTime)
		{
			_stateTime += deltaTime;
			if (_state != null && _state.onUpdate != null) _state.onUpdate(deltaTime);
		}
	}
}