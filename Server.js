require('dotenv').config();
const express = require('express');
const cors = require('cors');
const mongoose = require('mongoose');

const app = express();
app.use(cors());
app.use(express.json());

// Connect to MongoDB
mongoose.connect(process.env.MONGO_URI, {
  useNewUrlParser: true,
  useUnifiedTopology: true
}).then(() => console.log("DB connected"))
  .catch(err => console.error(err));

// User schema
const userSchema = new mongoose.Schema({
  email: { type: String, unique: true, sparse: true },
  phone: { type: String, unique: true, sparse: true },
  passwordHash: String,
  verified: { type: Boolean, default: false }
});

const User = mongoose.model("User", userSchema);

// Register
app.post("/api/register", async (req, res) => {
  try {
    const { email, phone, password } = req.body;

    if (!email && !phone) return res.status(400).json({ error: "Email or phone required." });

    // Check duplicates
    if (email && await User.findOne({ email })) {
      return res.status(400).json({ error: "Email already registered." });
    }
    if (phone && await User.findOne({ phone })) {
      return res.status(400).json({ error: "Phone already registered." });
    }

    const hashed = await require('bcryptjs').hash(password, 10);

    const user = new User({ email, phone, passwordHash: hashed });
    await user.save();

    // TODO: send verification email/SMS here.
    res.status(201).json({ message: "Registered (verification pending)." });

  } catch (err) {
    res.status(500).json({ error: "Server error" });
  }
});

// Simple verification (tokenless for demo)
app.post("/api/verify", async (req, res) => {
  const { emailOrPhone } = req.body;
  const user = await User.findOne({
    $or: [{ email: emailOrPhone }, { phone: emailOrPhone }]
  });
  if (!user) return res.status(404).json({ error: "User not found" });

  user.verified = true;
  await user.save();
  res.json({ message: "User verified" });
});

const PORT = process.env.PORT || 5000;
app.listen(PORT, () => console.log(`Server running on ${PORT}`));
