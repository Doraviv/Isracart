import React, { Component } from 'react';
import { NavMenu } from './components/NavMenu';
import { AddAssignment } from './components/assignments/AddAssignment/AddAssignment';
import { AssignmentsList } from './components/assignments/AssignmentsList/AssignmentsList';

import './custom.css'

export default class App extends Component {
    static displayName = App.name;
    getAssignmentsForCurrentSessionPath = "https://localhost:44370/api/assignment/getAssignments";
    getAssignmentsForAllSessionsPath = "https://localhost:44370/api/assignment/getAllAssignments";

    constructor() {
        super();
        this.state = {
            assignments: null,
            isLoading: false
        };
    }

    //Load session assignments when view loads
    componentDidMount() {
        this.GetAssignments(this.getAssignmentsForCurrentSessionPath);
    }

    //Load session assignments after new assignment added
    HandleAddAssignment = () => {
        this.GetAssignments(this.getAssignmentsForCurrentSessionPath);
    };

    //Load assignments from all active sessions
    GetAssignmentsForAllSessions = () => {
        this.GetAssignments(this.getAssignmentsForAllSessionsPath);
    };

    //Get assignments from session or from all sessions depending on the path
    GetAssignments(path) {
        this.setState({
            isLoading: true
        });

        fetch(path)
            .then(response => {
                if (!response.ok) {
                    this.setState({
                        isLoading: false
                    });
                    throw Error(response.statusText);
                }
                return response.json();
            })
            .then(data => {
                this.setState({
                    assignments: data,
                    isLoading: false
                });
            }).catch((error) => {
                console.log(error);
            });
    }

    render () {
        return (
            <div>
                <NavMenu />
                <AddAssignment onAddAssignment={this.HandleAddAssignment}/>
                <AssignmentsList assignments={this.state.assignments} isLoading={this.state.isLoading} onLoadAllAssignments={this.GetAssignmentsForAllSessions}/>
            </div>
        );
    }
}
