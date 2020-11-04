import React, { Component } from 'react';
import './AssignmentItem.css';

export class AssignmentItem extends Component {
    static displayName = AssignmentItem.name;

    render() {
        return (
            <div>
                <div className="imageClass">
                    <img src={window.location.origin + "/" + this.props.file} alt={this.props.file} />
                </div>
                <h4>{this.props.name}</h4>
                <hr />
            </div>
        );
    }
}