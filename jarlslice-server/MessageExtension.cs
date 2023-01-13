using RiptideNetworking;
public static class MessageExtension {

    #region Color
    /// <inheritdoc cref="Add(Message, Color)"/>
    /// <remarks>Relying on the correct Add overload being chosen based on the parameter type can increase the odds of accidental type mismatches when retrieving data from a message. This method calls <see cref="Add(Message, Color)"/> and simply provides an alternative type-explicit way to add a <see cref="Color"/> to the message.</remarks>
    public static Message AddColor(this Message message, Color value) => Add(message, value);

    /// <summary>Adds a <see cref="Color"/> to the message.</summary>
    /// <param name="value">The <see cref="Color"/> to add.</param>
    /// <returns>The message that the <see cref="Color"/> was added to.</returns>
    public static Message Add(this Message message, Color value) {
        message.AddFloat(value.r);
        message.AddFloat(value.g);
        message.AddFloat(value.b);
        message.AddFloat(value.a);
        return message;
    }

    /// <summary>Retrieves a <see cref="Color"/> from the message.</summary>
    /// <returns>The <see cref="Color"/> that was retrieved.</returns>
    public static Color GetColor(this Message message) {
        return new Color(message.GetFloat(), message.GetFloat(), message.GetFloat(), message.GetFloat());
    }
    #endregion

    #region Vector3
    /// <inheritdoc cref="Add(Message, Vector3)"/>
    /// <remarks>Relying on the correct Add overload being chosen based on the parameter type can increase the odds of accidental type mismatches when retrieving data from a message. This method calls <see cref="Add(Message, Vector3)"/> and simply provides an alternative type-explicit way to add a <see cref="Vector3"/> to the message.</remarks>
    public static Message AddVector3(this Message message, Vector3 value) => Add(message, value);

    /// <summary>Adds a <see cref="Vector3"/> to the message.</summary>
    /// <param name="value">The <see cref="Vector3"/> to add.</param>
    /// <returns>The message that the <see cref="Vector3"/> was added to.</returns>
    public static Message Add(this Message message, Vector3 value) {
        message.AddFloat(value.x);
        message.AddFloat(value.y);
        message.AddFloat(value.z);
        return message;
    }

    /// <summary>Retrieves a <see cref="Vector3"/> from the message.</summary>
    /// <returns>The <see cref="Vector3"/> that was retrieved.</returns>
    public static Vector3 GetVector3(this Message message) {
        return new Vector3(message.GetFloat(), message.GetFloat(), message.GetFloat());
    }
    #endregion

}

public class Vector3 {
    public float x, y, z;
    public static Vector3 Zero = new Vector3(0,0,0);
    public Vector3(float x, float y, float z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

public class Color {
    public float r, g, b, a;
    public static Color White = new Color(0, 0, 0, 1);
    public Color(float r, float g, float b, float a) {
        this.r = r;
        this.b = b;
        this.g = g;
        this.a = a;
    }

    
}
