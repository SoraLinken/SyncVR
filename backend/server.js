// backend/server.js
const express = require('express');
const mongoose = require('mongoose');
const bodyParser = require('body-parser');

const app = express();
const port = 3000;

app.use(bodyParser.json());

mongoose.connect('mongodb://localhost:27017/yourdatabase', { useNewUrlParser: true, useUnifiedTopology: true });

const DataSchema = new mongoose.Schema({
    field1: String,
    field2: Number,
    // Add other fields as needed
});

const DataModel = mongoose.model('Data', DataSchema);

// POST endpoint to add data
app.post('/data', async (req, res) => {
    try {
        const data = new DataModel(req.body);
        await data.save();
        res.status(201).send(data);
    } catch (error) {
        res.status(400).send(error);
    }
});

// GET endpoint to retrieve data
app.get('/data', async (req, res) => {
    try {
        const data = await DataModel.find({});
        res.status(200).send(data);
    } catch (error) {
        res.status(400).send(error);
    }
});

app.listen(port, () => {
    console.log(`Server running at http://localhost:${port}`);
});
