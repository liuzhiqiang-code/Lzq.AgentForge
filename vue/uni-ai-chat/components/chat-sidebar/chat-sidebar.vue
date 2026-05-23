<template>
  <view class="chat-sidebar">
    <view class="sidebar-header">
      <text class="sidebar-title">聊天历史</text>
      <view class="sidebar-actions">
        <uni-icons @click="onNewChat" type="plus" size="22" color="#666"></uni-icons>
      </view>
    </view>
    
    <scroll-view class="sidebar-list" scroll-y="true">
      <view v-for="chat in chats" :key="chat.id" class="chat-item" :class="{'active': chat.id === currentChatId, 'pinned': chat.isTop}" @click="onChatClick(chat)">
        <view class="chat-item-content">
          <view class="chat-item-title">
            <text>{{ chat.title || '新对话' }}</text>
          </view>
          <view class="chat-item-meta">
            <text class="chat-item-time">{{ formatTime(chat.modificationTime) }}</text>
          </view>
        </view>
        <view class="chat-item-actions">
          <uni-icons @click.stop="onPinChat(chat)" :type="chat.isTop ? 'pushpin-fill' : 'pushpin'" size="18" color="#666"></uni-icons>
          <uni-icons @click.stop="onRenameChat(chat)" type="edit" size="18" color="#666"></uni-icons>
          <uni-icons @click.stop="onDeleteChat(chat)" type="trash" size="18" color="#666"></uni-icons>
        </view>
      </view>
    </scroll-view>
  </view>
</template>

<script>
export default {
  name: "chat-sidebar",
  props: {
    chats: {
      type: Array,
      default: () => []
    },
    currentChatId: {
      type: [String, Number],
      default: ''
    }
  },
  methods: {
    onNewChat() {
      this.$emit('new-chat')
    },
    
    onChatClick(chat) {
      this.$emit('chat-click', chat)
    },
    
    onPinChat(chat) {
      this.$emit('pin-chat', chat)
    },
    
    onRenameChat(chat) {
      this.$emit('rename-chat', chat)
    },
    
    onDeleteChat(chat) {
      this.$emit('delete-chat', chat)
    },
    
    formatTime(timeStr) {
      if (!timeStr) return ''
      const date = new Date(timeStr)
      return date.toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'})
    }
  }
}
</script>

<style scoped>
.chat-sidebar {
  width: 200rpx;
  border-right: 1px solid #eee;
  background-color: #fafafa;
  display: flex;
  flex-direction: column;
  height: 100%;
}

.sidebar-header {
  padding: 12px;
  border-bottom: 1px solid #eee;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.sidebar-title {
  font-size: 14px;
  font-weight: 500;
  color: #333;
}

.sidebar-actions {
  display: flex;
  gap: 8px;
}

.sidebar-list {
  flex: 1;
}

.chat-item {
  padding: 12px;
  cursor: pointer;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-radius: 6px;
  transition: background-color 0.2s;
}

.chat-item:hover {
  background-color: #f0f0f0;
}

.chat-item.active {
  background-color: #e3f2fd;
}

.chat-item.pinned {
  border-left: 3px solid #07c160;
}

.chat-item-content {
  flex: 1;
  display: flex;
  flex-direction: column;
}

.chat-item-title {
  font-size: 13px;
  color: #333;
  margin-bottom: 4px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.chat-item-meta {
  font-size: 11px;
  color: #999;
}

.chat-item-actions {
  display: flex;
  gap: 6px;
  align-items: center;
}
</style>