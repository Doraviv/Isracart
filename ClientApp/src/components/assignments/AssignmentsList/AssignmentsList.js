import React, { Component } from 'react';
import { AssignmentItem } from '../AssignmentItem/AssignmentItem';
import './AssignmentsList.css';

export class AssignmentsList extends Component {
    static displayName = AssignmentsList.name;

    constructor(props) {
        super(props);
        this.state = {
            assignments: this.props.assignments,
            isLoading: this.props.isLoading
        };
    }

    //Update state when recieving updates
    componentDidUpdate(prevProps, prevState, snapshot) {
        if (this.props.assignments !== this.state.assignments) {
            this.setState({
                assignments: this.props.assignments
            })
        }

        if (this.props.isLoading !== this.state.isLoading) {
            this.setState({
                isLoading: this.props.isLoading
            })
        }
    }

    //Button click - load assignments from all sessions
    hangleGetAssignmentsFromAllSessionsClick = () => {
        this.props.onLoadAllAssignments();
    };

    render() {
        const assignmentComponents =
            this.state.assignments != null && this.state.assignments.length > 0 ?
                this.state.assignments.map((assignment) => {
                    return (<AssignmentItem key={assignment.id} file={assignment.filePath} name={assignment.name} />);
                }) : null

        return (
            <div className="assignmentsList">
                {this.state.isLoading ? <h1>Loading...</h1> : <h1>Assignments list</h1>}
                <div className="assignmentListContainer">
                    {assignmentComponents != null ? <div>{assignmentComponents}</div> : <p>No assignments</p>}
                </div>
                <button type="button" className="btn btn-dark" onClick={this.hangleGetAssignmentsFromAllSessionsClick}>
                    Load assignments from all sessions
                </button>
            </div>
        );
    }
}
