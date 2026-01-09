namespace UIWidgets.Examples
{
	using System;
	using UIWidgets;
	using UnityEngine;
	using UnityEngine.Serialization;

	/// <summary>
	/// Origin of chat line.
	/// </summary>
	public enum ChatLineType
	{
		/// <summary>
		/// Incoming message.
		/// </summary>
		Incoming,

		/// <summary>
		/// Outgoing message.
		/// </summary>
		Outgoing,
	}

	/// <summary>
	/// Chat line.
	/// </summary>
	[Serializable]
	public class ChatLine : ObservableData, IEquatable<ChatLine>
	{
		[SerializeField]
		[FormerlySerializedAs("UserName")]
		string userName;

		/// <summary>
		/// Username.
		/// </summary>
		public string UserName
		{
			get => userName;

			set => Change(ref userName, value);
		}

		[SerializeField]
		[FormerlySerializedAs("Message")]
		string message;

		/// <summary>
		/// Message.
		/// </summary>
		public string Message
		{
			get => message;

			set => Change(ref message, value);
		}

		[SerializeField]
		[FormerlySerializedAs("Time")]
		DateTime time;

		/// <summary>
		/// Message time.
		/// </summary>
		public DateTime Time
		{
			get => time;

			set => Change(ref time, value);
		}

		[SerializeField]
		[FormerlySerializedAs("Image")]
		Sprite image;

		/// <summary>
		/// Attached image.
		/// </summary>
		public Sprite Image
		{
			get => image;

			set => Change(ref image, value);
		}

		[SerializeField]
		[FormerlySerializedAs("Audio")]
		AudioClip audio;

		/// <summary>
		/// Attached audio.
		/// </summary>
		public AudioClip Audio
		{
			get => audio;

			set => Change(ref audio, value);
		}

		[SerializeField]
		[FormerlySerializedAs("Type")]
		ChatLineType type;

		/// <summary>
		/// Message type.
		/// </summary>
		public ChatLineType Type
		{
			get => type;

			set => Change(ref type, value);
		}

		/// <summary>
		/// Convert this instance to string.
		/// </summary>
		/// <returns>String.</returns>
		public override string ToString()
		{
			return string.Format("{0} [{1}]: {2}", Time.ToString(), UserName, Message);
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="other">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public bool Equals(ChatLine other)
		{
			if (other == null)
			{
				return false;
			}

			return userName == other.userName
				&& message == other.message
				&& time == other.time
				&& image == other.image
				&& audio == other.audio
				&& type == other.type;
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj) => (obj is ChatLine item) && Equals(item);

		/// <summary>
		/// Compare specified objects.
		/// </summary>
		/// <param name="a">First object.</param>
		/// <param name="b">Second object.</param>
		/// <returns>true if the objects are equal; otherwise, false.</returns>
		public static bool operator ==(ChatLine a, ChatLine b) => (a is null) ? b is null : a.Equals(b);

		/// <summary>
		/// Compare specified objects.
		/// </summary>
		/// <param name="a">First object.</param>
		/// <param name="b">Second object.</param>
		/// <returns>true if the objects not equal; otherwise, false.</returns>
		public static bool operator !=(ChatLine a, ChatLine b) => !(a == b);

		/// <summary>
		/// Hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode() => HashCode.Combine(userName, message, image, audio, time, type);
	}
}