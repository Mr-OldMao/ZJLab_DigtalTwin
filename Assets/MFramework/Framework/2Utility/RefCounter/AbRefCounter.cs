namespace MFramework
{
    /// <summary>
    /// 标题：引用计数器实现类
    /// 功能：缓存资源的使用次数，资源引用次数为0时自动释放资源
    /// 作者：毛俊峰
    /// 时间：2022.07.24
    /// 版本：1.0
    /// </summary>
    public abstract class AbRefCounter : IRefCounter
    {
        /// <summary>
        /// 引用次数
        /// </summary>
        public int RefCount { get; private set; }

        /// <summary>
        /// 持有资源 引用次数+1
        /// </summary>
        /// <param name="refOwner"></param>
        public void Release(object refOwner = null)
        {
            RefCount++;
        }

        /// <summary>
        /// 释放资源 引用次数-1
        /// </summary>
        /// <param name="refOwner"></param>
        public void Retain(object refOwner = null)
        {
            if (RefCount > 0)
            {
                RefCount--;
                if (RefCount == 0)
                {
                    OnZeroRef();
                }
            }
        }

        /// <summary>
        /// 引用次数为0调用
        /// </summary>
        protected abstract void OnZeroRef();
    }
}