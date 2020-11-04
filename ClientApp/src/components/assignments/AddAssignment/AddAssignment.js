import React, { Component } from 'react';
import axios from 'axios';
import './AddAssignment.css';

export class AddAssignment extends Component {
    static displayName = AddAssignment.name;

    constructor(props) {
        super(props);

        this.state = {
            description: "",
            file: null,
            errors: {}
        };
    }

    //Update state when input changed
    handleInputChange = (event) => {
        const { name, value } = event.target;

        this.setState({
            [name]: value
        });
    }

    //Update state when file added or changed
    handleFileChange = (event) => {
        const { name, files } = event.target;

        this.setState({
            [name]: files[0]
        });
    }

    //Button click - add assignment process
    handleAddAssignmentClick = () => {
        var isValid = this.validateFields();

        if (isValid) {
            this.addAssignment()
            this.resetState();
        }
    };

    //Validate input fields
    validateFields() {
        let isValid = true;
        let errors = {};

        //Description
        if (!this.state.description) {
            errors["description"] = "Description Cannot be empty";
            isValid = false;
        }

        //File
        if (!this.state.file) {
            isValid = false;
            errors["fileInput"] = "Please Add File";
        }

        this.setState({ errors: errors });

        return isValid;
    }   

    //Reset state and file input to default value
    resetState() {
        this.setState({
            description: "",
            file: null,
            errors: {}
        });

        document.getElementById("fileInput").value = "";
    }

    //Http request to save the assignment
    addAssignment(e) {
        const formData = new FormData();
        formData.append("File", this.state.file);
        formData.append("Description", this.state.description);

        axios.post("https://localhost:44370/api/assignment/setAssignment", formData, {
            headers: { 'Content-Type': 'multipart/form-data' }
        }).then((res) => {
            this.props.onAddAssignment();
        }).catch(error => console.log(error));
    }

    render () {
        return (
            <div className="addAssignment">
                <h1>Add Assignment</h1>
                <div className="inputDescription">
                    <input
                        type="text"
                        name="description"
                        value={this.state.description}
                        placeholder="Assignment description"
                        onChange={this.handleInputChange} />
                    <span className="error">{this.state.errors["description"]}</span>
                    <div className="space"></div>
                </div>

                <input id="fileInput" type="file" name="file" onChange={this.handleFileChange} />
                <div>
                    <span className="error">{this.state.errors["fileInput"]}</span>
                </div>

                <hr />

                <button type="button" className="btn btn-primary" onClick={this.handleAddAssignmentClick}>
                    Add Assignment!
                </button> 
            </div>
        );
    }
}
